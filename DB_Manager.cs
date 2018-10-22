using CarLeasingViewer.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CarLeasingViewer
{
    public class DB_Manager
    {
        static LogSet m_loger = LogManager.GetDefaultLogSet<DB_Manager>();

        readonly string m_connectionString;

        private bool? m_Available;
        const string invoice = "Invoice ";

        public bool Available
        {
            get
            {
                if (m_Available == null)
                    TryConnect();

                return (bool)m_Available;
            }
        }

        public static DB_Manager Default { get { return new DB_Manager(Properties.Settings.Default.ConnectionString); } }

        public DB_Manager(string connectionString)
        {
            m_connectionString = connectionString;

            TryConnect();
        }

        void TryConnect()
        {
            try
            {
                using (var connection = new SqlConnection(m_connectionString))
                    connection.Open();

                m_Available = true;
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при проверке подключения к БД", ex);
                m_Available = false;
            }
        }

        /// <summary>
        /// Получение списка доступных методов
        /// </summary>
        /// <param name="invoice">Флаг поиска в архиве</param>
        /// <returns>Возвращает список месяцев</returns>
        public IEnumerable<Month> GetAvailableMonthes(SearchSettings settings = null, int year = 0)
        {
            List<Month> monthes = new List<Month>();
            if (settings == null)
                settings = App.SearchSettings;


            #region Old


            var sql = string.Empty; //$@"SELECT DISTINCT
                                    //    h.[Date Begin]
                                    //    FROM [CARLSON_Test_10052018].[dbo].[Carlson$Sales{(invoice ? " Invoice" : "")} Line] l
                                    //    inner join [CARLSON_Test_10052018].[dbo].[Carlson$Sales{(invoice ? " Invoice" : "")} Header] h on h.[Sell-to Customer No_] = l.[Sell-to Customer No_]
                                    //    WHERE l.[Vehicle Reg_ No_] != ''
                                    //    AND h.[Date End] > h.[Date Begin]
                                    //    order by h.[Date Begin]";

            #endregion

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("SELECT DISTINCT (");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetAvailableMonthesQuery(oldSettings, year));
                sb.Append("\r\n UNION \r\n"); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetAvailableMonthesQuery(curentSettings, year));
                sb.Append(")");

                sql = sb.ToString();
            }
            else
                sql = GetAvailableMonthesQuery(settings, year);

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            year = (int)reader["Year"];

                            if (year > 2016)
                                monthes.Add(new Month(year, (Monthes)(int)reader["Month"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе доступных месяцев из БД", ex);
            }

            return monthes;
        }

        public MonthBusiness GetBusinessByMonth(Month month, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            var carBusinesses = new List<CarBusiness>();

            #region Old

            /*
            var sql = $@"SELECT DISTINCT
	                                  l.[Document No_]
	                                ,h.[Salesperson Code]
	                                ,h.[Bal_ Account No_]
	                                ,h.[Sell-to Customer Name] as Buyer
	                                ,h.[Bill-to Name]
	                                ,h.[Ship-to Name]
                                    ,l.[Description] as CarName
                                    ,l.[Vehicle Reg_ No_] as CarNumber
	                                ,h.[Venicle Operation Area]
	                                ,h.[Date Begin] as DateStart
	                                ,h.[Time Begin]
	                                ,h.[Date End] as DateEnd
	                                ,h.[Time End]
	                                ,h.[Comment Text] as Comment
                                FROM [CARLSON_Test_10052018].[dbo].[Carlson$Sales{(invoice ? " Invoice" : "")} Line] l
                                INNER JOIN [CARLSON_Test_10052018].[dbo].[Carlson$Sales{(invoice ? " Invoice" : "")} Header] h ON h.[Sell-to Customer No_] = l.[Sell-to Customer No_]
                                WHERE l.[Vehicle Reg_ No_] != ''
                                AND h.[Date End] > h.[Date Begin]
                                ORDER BY l.[Document No_], h.[Date Begin]";
                                */
            #endregion

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("SELECT DISTINCT (");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetBusinessByMonthQuery(month, oldSettings, region));
                sb.Append("\r\n UNION \r\n"); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetBusinessByMonthQuery(month, curentSettings, region));
                sb.Append(")");

                sql = sb.ToString();
            }
            else
                sql = GetBusinessByMonthQuery(month, settings, region);
            //sql = $@"SELECT 
            //             i.[No_]
            //            , i.[Description] as CarName
            //            , i.[Vehicle Reg_ No_] as CarNumber
            //            , i.[Blocked]
            //            , l.[Document No_]
            //         ,h.[Salesperson Code]
            //         ,h.[Bal_ Account No_]
            //         ,h.[Sell-to Customer Name] as Buyer
            //         ,h.[Bill-to Name]
            //         ,h.[Ship-to Name]
            //         ,h.[Venicle Operation Area]
            //         ,h.[Date Begin] as DateStart
            //         ,h.[Time Begin]
            //         ,h.[Date End] as DateEnd
            //         ,h.[Time End]
            //         ,h.[Comment Text] as Comment
            //             FROM Carlson$Item i
            //            	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
            //            	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]

            //            WHERE 1 = 1
            //                {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
            //            	AND i.IsService = 0
            //            	AND i.IsFranchise = 0
            //                AND h.[Date Begin] IS NOT NULL
            //                {(region == null || string.IsNullOrWhiteSpace(region.DBKey) ? "" : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
            //                AND ((h.[Date Begin] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}'))

            //            ORDER BY l.[Document No_]";

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var curentCar = string.Empty;
                        var previosCar = string.Empty;
                        var buyer = string.Empty;
                        CarBusiness cb = null;
                        while (reader.Read())
                        {
                            curentCar = (string)reader["CarName"];

                            var no_ = (string)reader["No_"];

                            cb = carBusinesses.FirstOrDefault(_cb => _cb.ItemNo.Equals(no_));
                            if (cb == null)
                            {
                                previosCar = curentCar;

                                cb = new CarBusiness();
                                cb.Month = month;
                                cb.ItemNo = (string)reader["No_"];
                                carBusinesses.Add(cb);

                                var number = ((string)reader["CarNumber"]).Trim();
                                var lastCharIndex = number.Last(c => char.IsLetter(c)) + 1;

                                if ((lastCharIndex) < number.Length)
                                    number = number.Substring(lastCharIndex, number.Length - lastCharIndex);

                                cb.Name = $"{curentCar} ({number})";
                            }

                            buyer = (string)reader["Buyer"];

                            if (cb.Count > 0 && cb.Business.Last().Title.Equals(buyer))
                                cb.Business.Last().DateEnd = ((DateTime)reader["DateEnd"]);
                            else
                            {
                                var b = new Leasing();
                                b.DateStart = ((DateTime)reader["DateStart"]);
                                b.DateEnd = ((DateTime)reader["DateEnd"]);
                                b.Title = buyer;
                                b.Type = BusinessType.Leasing;
                                b.Comment = (string)reader["Comment"];
                                b.CurrentMonth = month;

                                cb.Add(b);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", ex);
            }

            var monthBusiness = new MonthBusiness(carBusinesses);
            monthBusiness.Month = month;

            return monthBusiness;
        }

        public MonthBusiness GetBusinessByMonthes(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            var carBusinesses = new List<CarBusiness>();

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("SELECT DISTINCT ").Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetBusinessByMonthesQuery(start, end, oldSettings, region));
                //sb.Append(")");
                sb.Append("\r\n UNION \r\n");//.Append("("); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetBusinessByMonthesQuery(start, end, curentSettings, region));
                sb.Append(")");//.Append(" ORDER BY [Document No_]");

                sql = sb.ToString();
            }
            else
                sql = GetBusinessByMonthesQuery(start, end, settings, region);

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var curentCar = string.Empty;
                        var previosCar = string.Empty;
                        var buyer = string.Empty;
                        CarBusiness cb = null;
                        while (reader.Read())
                        {
                            curentCar = (string)reader["CarName"];

                            var no_ = (string)reader["No_"];

                            cb = carBusinesses.FirstOrDefault(_cb => _cb.ItemNo.Equals(no_));
                            if (cb == null)
                            {
                                previosCar = curentCar;

                                cb = new CarBusiness();
                                cb.Monthes = Month.GetMonthes(new DateTime(start.Year, start.Index, 1), new DateTime(end.Year, end.Index, 1));
                                cb.ItemNo = (string)reader["No_"];
                                carBusinesses.Add(cb);

                                var number = ((string)reader["CarNumber"]).Trim();
                                var lastCharIndex = number.Last(c => char.IsLetter(c)) + 1;

                                if ((lastCharIndex) < number.Length)
                                    number = number.Substring(lastCharIndex, number.Length - lastCharIndex);

                                cb.Name = $"{curentCar} ({number})";
                            }

                            buyer = (string)reader["Buyer"];

                            if (cb.Count > 0 && cb.Business.Last().Title.Equals(buyer))
                                cb.Business.Last().DateEnd = ((DateTime)reader["DateEnd"]);
                            else
                            {
                                var b = new Leasing();
                                b.DateStart = ((DateTime)reader["DateStart"]);
                                b.DateEnd = ((DateTime)reader["DateEnd"]);
                                b.Title = buyer;
                                b.Type = BusinessType.Leasing;
                                b.Comment = (string)reader["Comment"];
                                b.Monthes = Month.GetMonthes(b.DateStart, b.DateEnd);
                                b.Saler = (string)reader["Saler"];
                                b.Blocked = (bool)reader["Blocked"];

                                cb.Add(b);
                            }
                        }
                    }
                }
            }
            catch(SqlException sqlEx)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", sqlEx,
                    new LogParameter("Запрос", sql));
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", ex);
            }

            var monthBusiness = new MonthBusiness(carBusinesses);
            monthBusiness.Monthes = Month.GetMonthes(new DateTime(start.Year, start.Index, 1), new DateTime(end.Year, end.Index, 1));

            return monthBusiness;
        }


        public IEnumerable<Region> GetRegions()
        {
            List<Region> regions = new List<Region>();

            if (App.SearchSettings.TestData)
            {
                regions = new List<Region>()
                {
                    new Region(){ Address = "ул.Краснококшайская, д.72", DBKey = "KZ", DisplayName = "Казань", PostCode = "420032" }
                    , new Region(){ Address = "ул.Ахметшина,автостоянка \"Вектор-Ч\"", DBKey = "NCH", DisplayName = "Набережные Челны", PostCode = "423814" }
                    , new Region(){ Address = "ул. М.Ямская, д.78А", DBKey = "NN", DisplayName = "Нижний Новгород", PostCode = "603022" }
                };
            }
            else
            {
                try
                {
                        var sql = $@"SELECT [Code]
                                	    , [Name]
                                	    , [Address]
                                	    , [Post Code]
                                     FROM [Carlson$Responsibility Center]";

                    using (var con = new SqlConnection(m_connectionString))
                    {
                        var com = new SqlCommand(sql);
                        com.Connection = con;

                        con.Open();

                        using (var reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var r = new Region();
                                r.DBKey = (string)reader["Code"];
                                r.DisplayName = (string)reader["Name"];
                                r.Address = (string)reader["Address"];
                                r.PostCode = (string)reader["Post code"];
                                regions.Add(r);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    m_loger.Log("Возникло исключение при получении списка регионов из БД", ex);
                }
            }

            return regions;
        }

        string GetAvailableMonthesQuery(SearchSettings settings, int year = 0)
        {
            if (settings == null)
                settings = App.SearchSettings;

            return $@"SELECT DISTINCT 
                            MONTH(h.[Date Begin]) as Month
                            , YEAR(h.[Date Begin]) as Year
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {(year > 0 ? ("AND YEAR(h.[Date Begin]) = " + year.ToString()) : "")}
                        
                        ORDER BY Year";
        }

        String GetBusinessByMonthesQuery(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            return $@"SELECT 
                         i.[No_]
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , l.[Document No_]
	                    ,h.[Salesperson Code] as Saler
	                    ,h.[Bal_ Account No_]
	                    ,h.[Sell-to Customer Name] as Buyer
	                    ,h.[Bill-to Name]
	                    ,h.[Ship-to Name]
	                    ,h.[Venicle Operation Area]
	                    ,h.[Date Begin] as DateStart
	                    ,h.[Time Begin]
	                    ,h.[Date End] as DateEnd
	                    ,h.[Time End]
	                    ,h.[Comment Text] as Comment
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {(region == null || string.IsNullOrWhiteSpace(region.DBKey) ? "" : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
                            AND ((h.[Date Begin] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}'))";
        }

        String GetBusinessByMonthQuery(Month month, SearchSettings settings = null, Region region = null)
        {
            return $@"SELECT 
                         i.[No_]
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , l.[Document No_]
	                    ,h.[Salesperson Code] as Saler
	                    ,h.[Bal_ Account No_]
	                    ,h.[Sell-to Customer Name] as Buyer
	                    ,h.[Bill-to Name]
	                    ,h.[Ship-to Name]
	                    ,h.[Venicle Operation Area]
	                    ,h.[Date Begin] as DateStart
	                    ,h.[Time Begin]
	                    ,h.[Date End] as DateEnd
	                    ,h.[Time End]
	                    ,h.[Comment Text] as Comment
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {(region == null || string.IsNullOrWhiteSpace(region.DBKey) ? "" : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
                            AND ((h.[Date Begin] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}'))
                        
                        ORDER BY l.[Document No_]";
        }
    }
}
