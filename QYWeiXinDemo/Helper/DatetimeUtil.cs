using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QYWeiXinDemo.Helper
{
    class DatetimeUtil
    {
        /// <summary>
        /// 此计算机上的当前日期和时间，表示为协调世界时的DateTime对象转化为数据库长整型
        /// </summary>
        /// <returns></returns>
        public static long GetInternalTicke()
        {
            var date = DateTime.UtcNow;
            return GetInternalTickeWithoutTimeZone(date);
        }

        private static long GetInternalTickeWithoutTimeZone(DateTime datetime)
        {
            //CAST(10000000 AS BIGINT)*datediff(s,'2000-01-01 00:00:00' ,getutcdate()) + 630822816000000000
            //dateadd(s,(635838528800000000/10000000-630822816000000000/10000000),'2000-1-1')
            var date = DateTime.Parse("2000-01-01 00:00:00");
            var inputDate = DateTime.Parse(datetime.ToShortDateString() + " " + datetime.ToLongTimeString());
            long localtick = (inputDate.Ticks - 630822816000000000) + date.Ticks;
            return localtick;
        }
    }
}
