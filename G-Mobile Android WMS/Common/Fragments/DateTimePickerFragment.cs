using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace G_Mobile_Android_WMS.Fragments
{
#pragma warning disable CS0618
    public class DatePickerFragment : DialogFragment,
                                      DatePickerDialog.IOnDateSetListener
    {
        Action<DateTime> _dateSelectedHandler = delegate { };
        DateTime Default;

        public static DatePickerFragment NewInstance(Action<DateTime> onDateSelected, DateTime DefaultDate)
        {
            DatePickerFragment frag = new DatePickerFragment();
            frag._dateSelectedHandler = onDateSelected;
            frag.Default = DefaultDate;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                           this,
                                                           Default.Year,
                                                           Default.Month - 1,
                                                           Default.Day);
            return dialog;
        }

        public void OnDateSet(DatePicker view, int Year, int Month, int Day)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(Year, Month + 1, Day);
            _dateSelectedHandler(selectedDate);
        }
    }
#pragma warning restore CS0618
}