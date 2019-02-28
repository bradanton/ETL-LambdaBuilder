﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using org.ohdsi.cdm.framework.common2.Base;
using org.ohdsi.cdm.framework.common2.Builder;
using org.ohdsi.cdm.framework.common2.Definitions;
using org.ohdsi.cdm.framework.common2.Enums;
using org.ohdsi.cdm.framework.common2.Extensions;
using org.ohdsi.cdm.framework.common2.Omop;

namespace org.ohdsi.cdm.presentation.lambdabuilder.Base
{
    public class LambdaChunkPart : IDisposable
    {
        private readonly Dictionary<long, Lazy<IPersonBuilder>> _personBuilders;
        private ChunkData _readyToSave = new ChunkData();

        private readonly int _chunkId;
        private readonly Func<IPersonBuilder> _createPersonBuilder;
        private readonly string _prefix;
        private readonly int _attempt;
        private long _totalBuild;
        private long _totalSave;
        private long _totalGc;

        private readonly KeyMasterOffsetManager _offsetManager;
        private long? _lastSavedPersonId;

        private readonly System.Timers.Timer _watchdog;
        private readonly Dictionary<string, S3DataReader2> _readers = new Dictionary<string, S3DataReader2>();
        private Dictionary<string, long> _restorePoint = new Dictionary<string, long>();

        public LambdaChunkPart(int chunkId, Func<IPersonBuilder> createPersonBuilder, string prefix, int attempt)
        {
            _chunkId = chunkId;
            _createPersonBuilder = createPersonBuilder;
            _prefix = prefix;
            _attempt = attempt;
            
            _personBuilders = new Dictionary<long, Lazy<IPersonBuilder>>();
            _offsetManager = new KeyMasterOffsetManager(_chunkId, int.Parse(_prefix), attempt);
            _lastSavedPersonId = null;
            _watchdog = new System.Timers.Timer(Settings.Current.WatchdogValue);
            _watchdog.Elapsed += _watchdog_Elapsed;
        }

        public int TotalPersonConverted { get; private set; }

        private void _watchdog_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Settings.Current.ReadIdle)
            {
                Settings.Current.Error = true;
                Settings.Current.WatchdogTimeout = true;

                _watchdog.Stop();
                Console.Write("*** watchdog timeout ***");
                foreach (var r in _readers.Values)
                {
                    r.Close();
                }
            }

            Settings.Current.ReadIdle = true;
        }


        public long? Process(Dictionary<string, long> restorePoint)
        {
            try
            {
                Console.WriteLine("--- RAM: " + GC.GetTotalMemory(false) / 1024 / 1024 + "Mb");
                _restorePoint = restorePoint;

                if(_restorePoint.ContainsKey("_lastSavedPersonId"))
                {
                    _lastSavedPersonId = _restorePoint["_lastSavedPersonId"];
                }

                var timer = new Stopwatch();
                timer.Start();

                var folder = $"{Settings.Current.Building.Vendor}/{Settings.Current.Building.Id}/raw";


                var qds = new Dictionary<string, QueryDefinition>();
                var queries = new List<string>();


                foreach (var qd in Settings.Current.Building.SourceQueryDefinitions)
                {
                    if (qd.Providers != null) continue;
                    if (qd.Locations != null) continue;
                    if (qd.CareSites != null) continue;

                    var sql = qd.GetSql(Settings.Current.Building.Vendor);

                    if (string.IsNullOrEmpty(sql))
                    {
                        qd.DataProcessed = true;
                        continue;
                    }

                    var initRow = 0L;
                    if (_restorePoint.ContainsKey(qd.FileName))
                        initRow = _restorePoint[qd.FileName];

                    _readers.Add(qd.FileName, new S3DataReader2(Settings.Current.Bucket, folder,
                        Settings.Current.S3AwsAccessKeyId,
                        Settings.Current.S3AwsSecretAccessKey, _chunkId, qd.FileName, qd.FieldHeaders, _prefix,
                        initRow));
                    qds.Add(qd.FileName, qd);
                    queries.Add(qd.FileName);
                }

                _watchdog.Start();
                var endedQueries = new List<string>();
                var maxPersonIds = new HashSet<long>();
                while (true)
                {
                    if (Settings.Current.Timeout || Settings.Current.Error)
                        break;

                    if (queries.Count == 0)
                        break;

                    for (int i = 0; i < queries.Count; i++)
                    {
                        var qName = queries[i];
                        var reader = _readers[qName];
                        var qd = qds[qName];

                        while (true)
                        {
                            if (!reader.Read())
                            {
                                endedQueries.Add(qName);
                                if (qd.ProcessedPersonIds.Count > 0)
                                    maxPersonIds.Add(qd.ProcessedPersonIds.Keys.Max());
                                break;
                            }

                            var recordGuid = Guid.NewGuid();

                            AddEntity(qd, qd.Persons, reader, recordGuid);
                            AddEntity(qd, qd.PayerPlanPeriods, reader, recordGuid);
                            AddEntity(qd, qd.Death, reader, recordGuid);
                            AddEntity(qd, qd.VisitOccurrence, reader, recordGuid);
                            AddEntity(qd, qd.Observation, reader, recordGuid);
                            AddEntity(qd, qd.ConditionOccurrence, reader, recordGuid);
                            AddEntity(qd, qd.ProcedureOccurrence, reader, recordGuid);
                            AddEntity(qd, qd.DrugExposure, reader, recordGuid);
                            AddEntity(qd, qd.Cohort, reader, recordGuid);
                            AddEntity(qd, qd.Measurement, reader, recordGuid);
                            AddEntity(qd, qd.DeviceExposure, reader, recordGuid);
                            AddEntity(qd, qd.Note, reader, recordGuid);

                            if (qd.ProcessedPersonIds.Count >= Settings.Current.MinPersonToBuild)
                            {
                                maxPersonIds.Add(qd.ProcessedPersonIds.Keys.Max());
                                break;
                            }
                        }
                    }

                    if (maxPersonIds.Count > 0)
                    {
                        var minPersonId = maxPersonIds.Min();
                        var ids = _personBuilders.Where(pb => pb.Key < minPersonId).Select(pb => pb.Key).ToArray();
                        BuildAndSave(ids, false);
                    }

                    foreach (var ended in endedQueries)
                    {
                        queries.Remove(ended);
                    }

                    endedQueries.Clear();
                    maxPersonIds.Clear();
                }

                _watchdog.Stop();

                if(_personBuilders.Keys.Count > 0)
                    BuildAndSave(_personBuilders.Keys.ToArray(), true);
                timer.Stop();

                Console.WriteLine(
                    $"(Timeout={Settings.Current.Timeout}) Total: {timer.ElapsedMilliseconds}ms, Build: {_totalBuild}ms, Save: {_totalSave}ms, GC: {_totalGc}ms || Total person: {TotalPersonConverted} || Duaration: {Settings.Current.Duration}s");

                foreach (var qd in Settings.Current.Building.SourceQueryDefinitions)
                {
                    qd.Clean();
                }

                foreach (var reader in _readers.Values)
                {
                    reader.Close();
                }

                _readers.Clear();
                _personBuilders.Clear();

                return Settings.Current.Timeout || Settings.Current.Error ? _lastSavedPersonId : null;
            }
            catch (Exception e)
            {
                Console.WriteLine("WARN_EXC - Process - throw");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Settings.Current.Error = true;
                throw;
            }
        }

        
        private void BuildAndSave(IReadOnlyCollection<long> ids, bool final)
        {
            if (Settings.Current.Timeout || Settings.Current.Error)
                return;

            if (ids.Count == 0)
                return;

            _watchdog?.Stop();

            var totalTime = new Stopwatch();
            totalTime.Start();

            var localRestorePoint = Build(ids);

            if (_readyToSave.Persons.Count >= Settings.Current.MinPersonToSave || final)
            {
                Save(localRestorePoint, totalTime);
            }
            
            _watchdog?.Start();
        }

        private void Save(Dictionary<string, long> localRestorePoint, Stopwatch totalTime)
        {
            if(_readyToSave.Persons.Count == 0)
                return;

            var personCount = _readyToSave.Persons.Count;
            TotalPersonConverted += personCount;

            _readyToSave.ChunkId = _chunkId;
            _readyToSave.SubChunkId = int.Parse(_prefix);

            var key = _prefix + "." + _attempt + "." + _readyToSave.Persons.Min(p => p.PersonId) + "-" +
                      _readyToSave.Persons.Max(p => p.PersonId) + "." + _readyToSave.Persons.Count;

            var timer = new Stopwatch();
            timer.Restart();
            var saver = new Saver(_chunkId, key);
            saver.Save(_readyToSave);

            _lastSavedPersonId = _readyToSave.Persons.Max(p => p.PersonId);

            foreach (var fileName in localRestorePoint.Keys)
            {
                if (!_restorePoint.ContainsKey(fileName))
                    _restorePoint.Add(fileName, 0);

                _restorePoint[fileName] = localRestorePoint[fileName];
            }

            
            if (!_restorePoint.ContainsKey("_lastSavedPersonId"))
                _restorePoint.Add("_lastSavedPersonId", 0);

            _restorePoint["_lastSavedPersonId"] = _lastSavedPersonId.Value;
            
            timer.Stop();

            var timeForSave = timer.ElapsedMilliseconds;
            _totalSave += timeForSave;

            _readyToSave.Clean();
            _readyToSave = null;
            _readyToSave = new ChunkData();

            totalTime.Stop();

            if (GC.GetTotalMemory(false) / 1024 / 1024 > 2200)
            {
                var gc = new Stopwatch();
                gc.Start();
                var msg = $"GC.Collect() => {GC.GetTotalMemory(false) / 1024 / 1024}Mb -> ";
                GC.Collect();
                gc.Stop();
                msg += $"{GC.GetTotalMemory(false) / 1024 / 1024}Mb || Duration: {gc.ElapsedMilliseconds}ms";
                _totalGc += gc.ElapsedMilliseconds;
                Console.WriteLine(msg);
            }

            Console.WriteLine(
                $"Save complete (total person={TotalPersonConverted}), part={key} Saved: {personCount} || Save: {timeForSave}ms, Total: {totalTime.ElapsedMilliseconds}ms || Last saved PersonId={_lastSavedPersonId} || RAM: {GC.GetTotalMemory(false) / 1024 / 1024}Mb || Duaration: {Settings.Current.Duration}s");

            Console.WriteLine("Restore point - " +
                              string.Join(';', _restorePoint.Select(rp => $"{rp.Key}:{rp.Value}")));
        }

        private Dictionary<string, long> Build(IEnumerable<long> ids)
        {
            var localRestorePoint = new Dictionary<string, long>();
            var timer = new Stopwatch();
            timer.Restart();
            var GCCollected = false;
            foreach (var personId in ids)
            {
                if (_personBuilders.Remove(personId, out var pb))
                {
                    _readyToSave.AddAttrition(personId, pb.Value.Build(_readyToSave, _offsetManager));
                    if (!GCCollected && GC.GetTotalMemory(false) / 1024 / 1024 > 1500)
                    {
                        var gc = new Stopwatch();
                        gc.Start();
                        var msg = $"GC.Collect() => {GC.GetTotalMemory(false) / 1024 / 1024}Mb -> ";
                        GC.Collect();
                        gc.Stop();
                        msg += $"{GC.GetTotalMemory(false) / 1024 / 1024}Mb || Duration: {gc.ElapsedMilliseconds}ms";
                        _totalGc += gc.ElapsedMilliseconds;
                        Console.WriteLine(msg);
                        GCCollected = true;
                    }
                }

                foreach (var qd in Settings.Current.Building.SourceQueryDefinitions)
                {
                    if (!qd.ProcessedPersonIds.ContainsKey(personId))
                        continue;

                    var rowIndex = qd.ProcessedPersonIds[personId];
                    if (!localRestorePoint.ContainsKey(qd.FileName))
                        localRestorePoint.Add(qd.FileName, 0);

                    if (localRestorePoint[qd.FileName] < rowIndex)
                        localRestorePoint[qd.FileName] = rowIndex;

                    qd.ProcessedPersonIds.Remove(personId);
                }
            }

            timer.Stop();

            var timeForBuilt = timer.ElapsedMilliseconds;
            _totalBuild += timeForBuilt;

            return localRestorePoint;
        }


        private void AddEntity(IEntity entity)
        {
            _personBuilders[entity.PersonId].Value.AddData(entity);
        }

        private void AddChildData(ProcedureOccurrence parent, ProcedureCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddChildData(DrugExposure parent, DrugCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddChildData(DeviceExposure parent, DeviceCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddChildData(Measurement parent, MeasurementCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddChildData(Observation parent, ObservationCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddChildData(VisitOccurrence parent, VisitCost child)
        {
            if (child.PaidCopay == 0 && child.PaidCoinsurance == 0 && child.PaidTowardDeductible == 0 &&
                child.PaidByPayer == 0 && child.TotalPaid == 0)
                return;

            _personBuilders[parent.PersonId].Value.AddChildData(parent, child);
        }

        private void AddEntity(QueryDefinition queryDefinition, IEnumerable<EntityDefinition> definitions,
            IDataRecord reader, Guid recordGuid)
        {
            if (definitions == null) return;

            foreach (var d in queryDefinition.FindDefinition(definitions, reader))
            {
                var personId = reader.GetLong(d.PersonId);

                if (_lastSavedPersonId.HasValue && personId <= _lastSavedPersonId) continue;

                queryDefinition.ProcessedPersonIds.TryAdd(personId.Value, 0);
                queryDefinition.ProcessedPersonIds[personId.Value] = ((S3DataReader2) reader).RowIndex;

                try
                {
                    Concept conceptDef = null;
                    if (d.Concepts != null && d.Concepts.Any())
                        conceptDef = d.Concepts[0];

                    bool added;
                    var pb =
                        _personBuilders.GetOrAdd(personId.Value,
                            key => new Lazy<IPersonBuilder>(() => _createPersonBuilder()),
                            out added).Value;

                    pb.JoinToVocabulary(d.Vocabulary);

                    foreach (var entity in d.GetConcepts(conceptDef, reader, _offsetManager))
                    {
                        if (entity == null) continue;

                        entity.SourceRecordGuid = recordGuid;
                        AddEntity(entity);

                        switch (entity.GeEntityType())
                        {
                            case EntityType.DrugExposure:
                            {
                                if (queryDefinition.DrugCost != null && queryDefinition.DrugCost[0].Match(reader))
                                    AddChildData((DrugExposure) entity,
                                        queryDefinition.DrugCost[0].CreateEnity((DrugExposure) entity, reader));
                                break;
                            }

                            case EntityType.ProcedureOccurrence:
                            {
                                if (queryDefinition.ProcedureCost != null &&
                                    queryDefinition.ProcedureCost[0].Match(reader))
                                    AddChildData((ProcedureOccurrence) entity,
                                        queryDefinition.ProcedureCost[0].CreateEnity((ProcedureOccurrence) entity,
                                            reader,
                                            _offsetManager));
                                break;
                            }

                            case EntityType.VisitOccurrence:
                            {
                                if (queryDefinition.VisitCost != null && queryDefinition.VisitCost[0].Match(reader))
                                    AddChildData((VisitOccurrence) entity,
                                        queryDefinition.VisitCost[0].CreateEnity((VisitOccurrence) entity, reader,
                                            _offsetManager));
                                break;
                            }

                            case EntityType.DeviceExposure:
                            {
                                if (queryDefinition.DeviceCost != null && queryDefinition.DeviceCost[0].Match(reader))
                                    AddChildData((DeviceExposure) entity,
                                        queryDefinition.DeviceCost[0].CreateEnity((DeviceExposure) entity, reader,
                                            _offsetManager));
                                break;
                            }

                            case EntityType.Observation:
                            {
                                if (queryDefinition.ObservationCost != null &&
                                    queryDefinition.ObservationCost[0].Match(reader))
                                    AddChildData((Observation) entity,
                                        queryDefinition.ObservationCost[0].CreateEnity((Observation) entity, reader));
                                break;
                            }

                            case EntityType.Measurement:
                            {
                                if (queryDefinition.MeasurementCost != null &&
                                    queryDefinition.MeasurementCost[0].Match(reader))
                                    AddChildData((Measurement) entity,
                                        queryDefinition.MeasurementCost[0].CreateEnity((Measurement) entity, reader));
                                break;
                            }
                        }
                    }

                    queryDefinition.RowProcessed();
                }
                catch (Exception e)
                {
                    Console.WriteLine("WARN_EXC - AddEntity - throw");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Settings.Current.Error = true;
                    throw;
                }
            }
        }

        public void Dispose()
        {
            _watchdog?.Dispose();

            if (_readers != null && _readers.Count > 0)
            {
                foreach (var reader in _readers.Values)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
    }
}
