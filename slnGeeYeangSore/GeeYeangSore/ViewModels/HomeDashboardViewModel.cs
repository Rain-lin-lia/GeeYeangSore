using System.Collections.Generic;

namespace GeeYeangSore.ViewModels
{
    public class HomeDashboardViewModel
    {
        // 統計數據
        public int PropertyCount { get; set; }        // 房源數
        public int NewUserCount { get; set; }         // 新增用戶數
        public decimal MonthlyIncome { get; set; }    // 本月收入

        // 重要提醒
        public int PendingPropertyCount { get; set; }        // 待審核房源數
        public int PendingIdentityCount { get; set; }        // 待審核身份驗證數 
        public int PendingReportCount { get; set; }          // 待處理檢舉數
        public int PendingLandlordCount { get; set; }      // 待審核房東數


        // 系統公告
        public List<SystemAnnouncement> SystemAnnouncements { get; set; }

        public HomeDashboardViewModel()
        {
            SystemAnnouncements = new List<SystemAnnouncement>();
        }
    }

    public class SystemAnnouncement
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
    }
}