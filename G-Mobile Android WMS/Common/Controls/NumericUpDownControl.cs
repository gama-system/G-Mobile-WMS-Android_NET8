using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.InputMethodServices;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Enums;
using Java.Lang;

namespace G_Mobile_Android_WMS.Controls
{
    public class NumericUpDown : LinearLayout
    {
        private EditText Field;
        private decimal _Value;
        public decimal Value
        {
            get { return _Value; }
            set
            {if (!Field.Enabled)
                    return;
                _Value = value;
                SetValue(_Value);
            }
        }

        private decimal _Min;
        public decimal Min
        {
            get { return _Min; }
            set
            {
                if (!Field.Enabled)
                    return;
                _Min = value;

                if (Value < _Min)
                    SetValue(_Min);
            }
        }

        private decimal _Max;
        public decimal Max
        {
            get { return _Max; }
            set
            {
                if (!Field.Enabled)
                    return;
                _Max = value;

                if (Value > _Max)
                    SetValue(_Max);
            }
        }

        private int _DecimalSpaces;
        public int DecimalSpaces
        {
            private get { return _DecimalSpaces; }
            set
            {
                _DecimalSpaces = value;
                SetValue(Value);
            }
        }

        public int Increment { get; set; }


        public NumericUpDown(Context context) : base(context)
        {
        }

        public NumericUpDown(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public NumericUpDown(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }

        public void Initialize()
        {
            try
            {
                _DecimalSpaces = Globalne.CurrentSettings.DecimalSpaces;
                _Min = 0;
                _Max = decimal.MaxValue;
                Increment = 1;

                Inflate(Context, Resource.Layout.numericupdown, this);

                FindViewById<Button>(Resource.Id.numupdown_btn_minus).Click += Minus_Click;
                FindViewById<Button>(Resource.Id.numupdown_btn_plus).Click += Plus_Click;

                Field = FindViewById<EditText>(Resource.Id.numupdown_edit);
                Field.Text = "0." + new string('0', _DecimalSpaces);
                Field.TextChanged += Field_TextChanged;
            }
            catch
            {
                // Kompatybilność z 5.0
            }
        }

        public void FocusField()
        {
            if (Field == null)
                return;

            Field.RequestFocus();
        }

        public void SetValue(decimal Val)
        {
            if (Field == null || !Field.Enabled)
                return;

            Field.Text = Val.ToString("0." + new string('0', _DecimalSpaces));
        }

        private void Field_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!Decimal.TryParse(Field.Text, out decimal NewVal))
            {
                SetValue(_Min);
            }
            else
            {
                string _newVal = NewVal.ToString("0.###");

                if (NewVal < _Min)
                    SetValue(_Min);
                else if (NewVal > _Max)
                    SetValue(_Max);
                else
                    _Value = decimal.Parse(_newVal);

                Field.KeyPress += Field_KeyPress;
            }
        }
        public void SetDisableControl()
        {
            Field.Enabled = false;
            FindViewById<Button>(Resource.Id.numupdown_btn_minus).Enabled = false;
            FindViewById<Button>(Resource.Id.numupdown_btn_plus).Enabled = false;
        }

        private void Field_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (!Field.Enabled)
                return;

            int cursorPosition = Field.SelectionStart;
            int selection = 5;

            if (e.Event.Action == KeyEventActions.Down)
            {
                if (cursorPosition < Field.Text.Length - 2)
                {
                    if (e.KeyCode == Android.Views.Keycode.Period)
                    {
                        string beforePeriod = string.Empty;
                        try
                        {
                            beforePeriod = Field.Text.Substring(0, Field.Text.IndexOf('.'));
                            if (beforePeriod.Length == 2) selection++;
                            if (beforePeriod.Length == 3) selection += 2;

                            Field.SetSelection(Field.Text.IndexOf('.') + 1);
                            Field.SetSelection(Field.SelectionStart, selection);
                        }
                        catch (System.Exception)
                        { }

                        
                    }
                }
                e.Handled = false;
            }
        }

        private void Plus_Click(object sender, EventArgs e)
        {
            SetValue(Value + Increment);
        }

        private void Minus_Click(object sender, EventArgs e)
        {
            SetValue(Value - Increment);
        }
    }
}