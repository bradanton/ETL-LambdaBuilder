﻿using System;
using System.Collections.Generic;
using System.Data;
using org.ohdsi.cdm.framework.common2.Builder;
using org.ohdsi.cdm.framework.common2.Extensions;
using org.ohdsi.cdm.framework.common2.Omop;

namespace org.ohdsi.cdm.framework.common2.DataReaders.v5
{
    public class DrugCostDataReader : IDataReader
    {
        private readonly IEnumerator<DrugCost> _enumerator;

        // A custom DataReader is implemented to prevent the need for the HashSet to be transformed to a DataTable for loading by SqlBulkCopy
        public DrugCostDataReader(List<DrugCost> batch)
        {
            _enumerator = batch.GetEnumerator();
        }

        public bool Read()
        {
            return _enumerator.MoveNext();
        }

        public int FieldCount
        {
            get { return 14; }
        }

        public object GetValue(int i)
        {
            if (_enumerator.Current == null) return null;

            switch (i)
            {

                case 0:

                    return KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId, _enumerator.Current.Id);

                case 1:

                    return KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId, _enumerator.Current.Id);

                case 2:
                    return _enumerator.Current.CurrencyConceptId;

                case 3:
                    return _enumerator.Current.PaidCopay.Round();

                case 4:
                    return _enumerator.Current.PaidCoinsurance.Round();

                case 5:
                    return _enumerator.Current.PaidTowardDeductible.Round();

                case 6:
                    return _enumerator.Current.PaidByPayer.Round();

                case 7:
                    return _enumerator.Current.PaidByCoordinationBenefits.Round();

                case 8:
                    return _enumerator.Current.TotalOutOfPocket.Round();

                case 9:
                    return _enumerator.Current.TotalPaid.Round();

                case 10:
                    return _enumerator.Current.IngredientCost.Round();

                case 11:
                    return _enumerator.Current.DispensingFee.Round();

                case 12:
                    return _enumerator.Current.AverageWholesalePrice.Round();

                case 13:
                    return _enumerator.Current.PayerPlanPeriodId.HasValue
                        ? KeyMasterOffsetManager.GetId(_enumerator.Current.PersonId, _enumerator.Current.PayerPlanPeriodId.Value)
                        : _enumerator.Current.PayerPlanPeriodId;

                default:
                    throw new NotImplementedException();
            }
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "Id";

                case 1:
                    return "DrugExposureId";

                case 2:
                    return "CurrencyConceptId";

                case 3:
                    return "PaidCopay";

                case 4:
                    return "PaidCoinsurance";

                case 5:
                    return "PaidTowardDeductible";

                case 6:
                    return "PaidByPayer";

                case 7:
                    return "PaidByCoordinationBenefits";

                case 8:
                    return "TotalOutOfPocket";

                case 9:
                    return "TotalPaid";

                case 10:
                    return "IngredientCost";

                case 11:
                    return "DispensingFee";

                case 12:
                    return "AverageWholesalePrice";

                case 13:
                    return "PayerPlanPeriodId";

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
                    return typeof(long?);

                case 3:
                    return typeof(decimal?);

                case 4:
                    return typeof(decimal?);

                case 5:
                    return typeof(decimal?);

                case 6:
                    return typeof(decimal?);

                case 7:
                    return typeof(decimal?);

                case 8:
                    return typeof(decimal?);

                case 9:
                    return typeof(decimal?);

                case 10:
                    return typeof(decimal?);

                case 11:
                    return typeof(decimal?);

                case 12:
                    return typeof(decimal?);

                case 13:
                    return typeof(long?);

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
