using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WellFitPlus.Common {

    public enum NotificationFrequency {
        [Description("20 Minutes")]
        Every_20 = 0,

        [Description("40 Minutes")]
        Every_40 = 1,

        [Description("60 Minutes")]
        Every_60 = 2,

        [Description("120 Minutes")]
        Every_120 = 3,
    }

    public enum VideoType {
        [Description("None")]
        None = -1,

        [Description("Intro")]
        Intro = 0,

        [Description("Trailer")]
        Trailer = 1,

        [Description("Exercise")]
        Exercise = 2
    }
}
