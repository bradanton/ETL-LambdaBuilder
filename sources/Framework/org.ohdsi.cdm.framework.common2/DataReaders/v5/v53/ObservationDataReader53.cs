﻿using System;
using System.Collections.Generic;
using System.Data;
using org.ohdsi.cdm.framework.common2.Builder;
using org.ohdsi.cdm.framework.common2.Omop;

namespace org.ohdsi.cdm.framework.common2.DataReaders.v5.v53
{
    public class ObservationDataReader53 : IDataReader
    {
        private readonly IEnumerator<Observation> _enumerator;

        // A custom DataReader is implemented to prevent the need for the HashSet to be transformed to a DataTable for loading by SqlBulkCopy
        public ObservationDataReader53(List<Observation> batch)
        {
            _enumerator = batch.GetEnumerator();
        }

        public bool Read()
        {
            return _enumerator.MoveNext();
        }

        public int FieldCount
        {
            get { return 18; }
        }

        // is this called only because the datatype specific methods are not implemented?  
        // probably performance to be gained by not passing object back?
        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId, _enumerator.Current.Id);
                case 1:
                    return _enumerator.Current.PersonId;
                case 2:
                    return _enumerator.Current.ConceptId;
                case 3:
                    return _enumerator.Current.StartDate;
                case 4:
                    return _enumerator.Current.Time;
                case 5:
                    return _enumerator.Current.TypeConceptId;
                case 6:
                    return _enumerator.Current.ValueAsNumber;
                case 7:
                    return _enumerator.Current.ValueAsString;
                case 8:
                    return _enumerator.Current.ValueAsConceptId;
                case 9:
                    return _enumerator.Current.QualifierConceptId;
                case 10:
                    return _enumerator.Current.UnitsConceptId;
                case 11:
                    return _enumerator.Current.ProviderId == 0 ? null : _enumerator.Current.ProviderId;
                case 12:
                    if (_enumerator.Current.VisitOccurrenceId.HasValue)
                    {
                        if (KeyMasterOffsetManager.GetKeyOffset(_enumerator.Current.PersonId).VisitOccurrenceIdChanged)
                            return KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId,
                                _enumerator.Current.VisitOccurrenceId.Value);

                        return _enumerator.Current.VisitOccurrenceId.Value;
                    }

                    return null;

                case 13:
                    if (_enumerator.Current.VisitDetailId.HasValue)
                    {
                        if (KeyMasterOffsetManager.GetKeyOffset(_enumerator.Current.PersonId).VisitDetailIdChanged)
                            return KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId,
                                _enumerator.Current.VisitDetailId.Value);

                        return _enumerator.Current.VisitDetailId;
                    }

                    return null;
                case 14:
                    return _enumerator.Current.SourceValue;
                case 15:
                    return _enumerator.Current.SourceConceptId;
                case 16:
                    return _enumerator.Current.UnitsSourceValue;
                case 17:
                    return _enumerator.Current.QualifierSourceValue;

                default:
                    throw new NotImplementedException();
            }
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0: return "observation_id";
                case 1: return "person_id";
                case 2: return "observation_concept_id";
                case 3: return "observation_date";
                case 4: return "observation_datetime";
                case 5: return "observation_type_concept_id";
                case 6: return "value_as_number";
                case 7: return "value_as_string";
                case 8: return "value_as_concept_id";
                case 9: return "qualifier_concept_id";
                case 10: return "unit_concept_id";
                case 11: return "provider_id";
                case 12: return "visit_occurrence_id";
                case 13: return "visit_detail_id";
                case 14: return "observation_source_value";
                case 15: return "observation_source_concept_id";
                case 16: return "unit_source_value";
                case 17: return "qualifier_source_value";

                default:
                    throw new NotImplementedException();
            }
        }

        #region implementationn not required for SqlBulkCopy

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte) GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char) GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime) GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal) GetValue(i);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(GetValue(i));
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(long);
                case 1:
                    return typeof(long);
                case 2:
                    return typeof(int);
                case 3:
                    return typeof(DateTime);
                case 4:
                    return typeof(string);
                case 5:
                    return typeof(int?);
                case 6:
                    return typeof(decimal?);
                case 7:
                    return typeof(string);
                case 8:
                    return typeof(int);
                case 9:
                    return typeof(int);
                case 10:
                    return typeof(int);
                case 11:
                    return typeof(long?);
                case 12:
                    return typeof(long?);
                case 13:
                    return typeof(long?);
                case 14:
                    return typeof(string);
                case 15:
                    return typeof(int);
                case 16:
                    return typeof(string);
                case 17:
                    return typeof(string);

                default:
                    throw new NotImplementedException();
            }
        }

        public float GetFloat(int i)
        {
            return (float) GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid) GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short) GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int) GetValue(i);
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(GetValue(i));
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return (string) GetValue(i);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
