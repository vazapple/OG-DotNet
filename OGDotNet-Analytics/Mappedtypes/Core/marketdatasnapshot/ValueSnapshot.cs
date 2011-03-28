﻿using System.ComponentModel;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.Core.marketdatasnapshot
{
    public class ValueSnapshot : INotifyPropertyChanged
    {
        private double _marketValue;
        private double? _overrideValue;

        public ValueSnapshot(double marketValue)
        {
            _marketValue = marketValue;
        }


        public double MarketValue
        {
            get { return _marketValue; }
            set
            {
                InvokePropertyChanged("MarketValue");
                _marketValue = value;
            }
        }

        public double? OverrideValue
        {
            get { return _overrideValue; }
            set
            {
                if (value != _overrideValue)
                {
                    _overrideValue = value;
                    InvokePropertyChanged("OverrideValue");
                }
            }
        }

        public static ValueSnapshot FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ValueSnapshot(ffc.GetDouble("marketValue").Value) {OverrideValue = ffc.GetDouble("overrideValue")};
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            if (_overrideValue != null)
            {
                a.Add("overrideValue", _overrideValue);
            }
            a.Add("marketValue", _marketValue);
        }

        private void InvokePropertyChanged(string propertyName)
        {
            InvokePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}