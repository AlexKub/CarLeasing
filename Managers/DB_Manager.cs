using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        #region queries



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

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetAvailableMonthesQuery(oldSettings, year));
                sb.Append(")").Append("\r\n UNION \r\n").Append("("); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetAvailableMonthesQuery(curentSettings, year));
                sb.Append(")");
                sb.Replace("ORDER BY Year", string.Empty);
                sb.Append("ORDER BY Year");

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
                m_loger.Log("Возникло исключение при запросе доступных месяцев из БД", ex,
                    new LogParameter("Запрос", sql));
            }

            return monthes;
        }

        public MonthBusiness GetBusinessByMonthes(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            var carBusinesses = new List<ItemInfo>();

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetBusinessByMonthesQuery(start, end, oldSettings, region));
                sb.Append(")");
                sb.Append("\r\n UNION \r\n").Append("("); //объединяем два запроса

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
                var selectingPeriod = new Period(start.FirstDate, end.LastDate);
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
                        ItemInfo cb = null;
                        bool includeNotActive = settings.IncludeNotActive;
                        while (reader.Read())
                        {
                            curentCar = (string)reader["CarName"];

                            var no_ = (string)reader["No_"];

                            cb = carBusinesses.FirstOrDefault(_cb => _cb.ID.Equals(no_));
                            if (cb == null)
                            {
                                previosCar = curentCar;

                                cb = new ItemInfo();
                                cb.Monthes = Month.GetMonthes(start, end);
                                cb.ID = (string)reader["No_"];
                                cb.OSAGO_END = (DateTime)reader["InsuranceEnd"];
                                cb.KASKO_END = (DateTime)reader["AddInsuranceEnd"];
                                try
                                {
                                    if (includeNotActive && IsMaintaining(reader))
                                    {
                                        var mi = ReadMaintenanceInfo(reader);
                                        //если пересекается с выбираемым периодом
                                        if (mi.Cross(selectingPeriod) && settings.IncludeNotActive)
                                            cb.Maintenance = mi;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    m_loger.Log("Возникло исключение при считывании Информации о ремонте из БД", ex);
                                }

                                carBusinesses.Add(cb);

                                var number = ((string)reader["CarNumber"])?.Trim() ?? string.Empty;

                                var lastCharIndex = number.Length > 0
                                    ? number.Last(c => char.IsLetter(c)) + 1 //ищем последнюю букву
                                    : 0;

                                if (lastCharIndex <= number.Length) //отрезаем регион
                                    number = number.Substring(lastCharIndex, number.Length - lastCharIndex);

                                cb.Name = $"{curentCar} ({number})";
                            }

                            buyer = (string)reader["Buyer"];

                            var b = new Leasing();
                            b.DateStart = ((DateTime)reader["DateStart"]).Add(((DateTime)reader["TimeStart"]).TimeOfDay);
                            b.DateEnd = ((DateTime)reader["DateEnd"]).Add(((DateTime)reader["TimeEnd"]).TimeOfDay);
                            b.Title = buyer;
                            b.Type = BusinessType.Leasing;
                            b.Comment = (string)reader["Comment"];
                            b.Monthes = Month.GetMonthes(b.DateStart, b.DateEnd);
                            b.Saler = (string)reader["Saler"];
                            b.Blocked = ((byte)reader["Blocked"]) > 0;

                            cb.Add(b);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", sqlEx,
                    new LogParameter("Запрос", sql));
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", ex,
                    new LogParameter("Запрос", sql));
            }

            //добавляем не занятые авто
            AddFreeCars(carBusinesses);

            var monthBusiness = new MonthBusiness(carBusinesses); //.OrderBy(cb => cb.Name)); сортировка теперь при выборке
            monthBusiness.Monthes = Month.GetMonthes(new DateTime(start.Year, start.Number, 1), new DateTime(end.Year, end.Number, 1));

            return monthBusiness;
        }

        /// <summary>
        /// Получение списка всех авто
        /// </summary>
        /// <param name="settings">Текущие настройки</param>
        /// <param name="region">Регион</param>
        /// <returns>Возвращает выбранный набор машин или пустой список</returns>
        public IEnumerable<Car> GetAllCars(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            if (region == null)
                region = settings.SelectedRegion;

            List<Car> cars = new List<Car>();
            try
            {
                var sql = $@"DECLARE @defultDate AS Datetime = '1753-01-01 00:00:00'
                        SELECT DISTINCT
                          i.[No_] as ID
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , p.[Unit Price] as Price
                        /*, u.[Active] as IsMaintaining
                        , u.[Date Begin] as MaintainanceStartDate
                        , u.[Time Degin] as MaintainanceStartTime
                        , u.[Date End] as MaintainanceEndDate
                        , u.[Time End] as MaintainanceEndTime
                        , u.[Description] as MaintainanceDescription*/

                         FROM Carlson$Item i
                            LEFT JOIN [Carlson$Sales Price] p ON p.[Item No_] = i.No_
                            LEFT JOIN [Carlson$Venicle Temp_ UnAvail_] u ON u.[Item No_] = i.No_
                    
                         WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                            /*{(settings.IncludeNotActive ? string.Empty : $"AND ((u.Active IS NULL) OR ((u.[Date Begin] > '{start.FirstDate.GetSqlDate()}') OR (u.[Date End] < '{end.Next().FirstDate.GetSqlDate()}')))")}*/
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND i.[IsPlasticCard] = 0
                            {((region == null || region.IsTotal) ? string.Empty : ("AND i.[Responsibility Center] = " + region.DBKey))}
                            AND p.[Minimum Quantity] = 1.0
							AND p.[Ending Date] = @defultDate
                        ORDER BY Price, ID";

                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var carName = string.Empty;
                        var carNumber = string.Empty;
                        var id = string.Empty;
                        Car car = null;
                        var includeBlocked = settings.IncludeBlocked;
                        var skipNotActive = !settings.IncludeNotActive;

                        while (reader.Read())
                        {
                            //if (skipNotActive && IsMaintaining(reader))
                            //    continue;

                            carName = (string)reader["CarName"];
                            carNumber = (string)reader["CarNumber"];
                            id = (string)reader["ID"];

                            car = new Car(carName, carNumber);
                            car.ID = id;

                            if (includeBlocked)
                                car.Blocked = ((byte)reader["Blocked"]) > 0;

                            cars.Add(car);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе полного набора машин из БД", ex);
            }

            return cars;
        }

        /// <summary>
        /// Получение текущей цены на машину
        /// </summary>
        /// <param name="car">Машина</param>
        /// <returns>Возвращает цены на переданную машину или ссылку по умолчанию</returns>
        public CarPriceList GetCarPrice(Car car)
        {
            if (car == null)
                return CarPriceList.Default;

            try
            {
                var sql = $@"DECLARE @defultDate AS Datetime = '1753-01-01 00:00:00'
                            SELECT
                                  [Minimum Quantity] AS Quantity
                                  ,[Unit Price] AS Price
                              FROM [Carlson$Sales Price] AS p
                            	WHERE p.[Ending Date] = @defultDate
                            		AND p.[Item No_] = '{car.ID}'";

                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        CarPriceList price = null;
                        decimal val;
                        int quantity;
                        var values = new decimal[3];

                        //получаем 3 строки с Срок | Цена
                        while (reader.Read())
                        {
                            if (price == null)
                                price = CarPriceList.Default;

                            //считываем срок (1, 3 или 7)
                            quantity = (int)(decimal)reader["Quantity"];

                            val = (decimal)reader["Price"];

                            switch (quantity)
                            {
                                case 1:
                                    //от 1 до 2 дней
                                    values[0] = val;
                                    break;
                                case 3:
                                    //от 3 до 6
                                    values[1] = val;
                                    break;
                                case 7:
                                    //от 7
                                    values[2] = val;
                                    break;
                                default: //хрен знает что пришло
                                    break;
                            }
                        }

                        if (price != null)
                            return new CarPriceList(values[0], values[1], values[2]);
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе цены на машину из БД", ex
                    , new LogParameter("No_", car?.ID));
            }

            return CarPriceList.Default;
        }

        public IEnumerable<Region> GetRegions()
        {
            List<Region> regions = new List<Region>();


            if (App.TestMode && App.SearchSettings.TestData)
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
            regions.Insert(0, Region.Total);

            return regions;
        }

        /// <summary>
        /// Получение порядка цен на авто
        /// </summary>
        /// <returns>Возвращает набор [ID, Price]</returns>
        public IEnumerable<Tuple<string, decimal>> GetDayPriceOrder()
        {
            List<Tuple<string, decimal>> prices = new List<Tuple<string, decimal>>();

            try
            {
                var sql = @"DECLARE @defaultDate AS Datetime = '1753-01-01 00:00:00'
                            SELECT
                                p.[Item No_] AS ID
                                , p.[Unit Price] AS Price
                            FROM [Carlson$Sales Price] AS p
                                LEFT JOIN [Carlson$Item] i ON i.[No_] = p.[Item No_]
                            WHERE 1 = 1
                                AND i.[IsService] = 0
                                AND i.[IsFranchise] = 0
                                AND i.[IsPlasticCard] = 0
                                AND p.[Ending Date] = @defaultDate
                            ORDER BY p.[Item No_]";

                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prices.Add(new Tuple<string, decimal>((string)reader["ID"], (decimal)reader["Price"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе порядка цен на авто из БД", ex);
            }

            return prices;
        }

        #endregion

        #region private        

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
                            AND i.[IsPlasticCard] = 0
                            AND h.[Date Begin] IS NOT NULL
                            {(year > 0 ? ("AND YEAR(h.[Date Begin]) = " + year.ToString()) : "")}
                        
                        ORDER BY Year";
        }

        String GetBusinessByMonthesQuery(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            if (region == null)
                region = settings.SelectedRegion;

            return $@"SELECT 
                         i.[No_]
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , i.[Venicle Insurance Date End] as InsuranceEnd
                        , i.[Venicle Add_Insurance Date End] as AddInsuranceEnd
                        , l.[Document No_]
	                    , h.[Salesperson Code] as Saler
	                    , h.[Bal_ Account No_]
	                    , h.[Sell-to Customer Name] as Buyer
	                    , h.[Bill-to Name]
	                    , h.[Ship-to Name]
	                    , h.[Venicle Operation Area]
	                    , h.[Date Begin] as DateStart
	                    , h.[Time Begin] as TimeStart
	                    , h.[Date End] as DateEnd
	                    , h.[Time End] as TimeEnd
	                    , h.[Comment Text] as Comment
                        , {(settings.SelectedDBSearchType == DBSearchType.Curent ? "0" : "1")} as Invoice
                        , u.[Active] as IsMaintaining
                        , u.[Date Begin] as MaintainanceStartDate
                        , u.[Time Degin] as MaintainanceStartTime
                        , u.[Date End] as MaintainanceEndDate
                        , u.[Time End] as MaintainanceEndTime
                        , u.[Description] as MaintainanceDescription
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                            LEFT JOIN [Carlson$Venicle Temp_ UnAvail_] u ON u.[Item No_] = i.No_
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                            {(settings.IncludeNotActive ? string.Empty : $"AND ((u.Active IS NULL) OR ((u.[Date Begin] > '{start.FirstDate.GetSqlDate()}') OR (u.[Date End] < '{end.Next().FirstDate.GetSqlDate()}')))")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND i.[IsPlasticCard] = 0
                            AND h.[Date Begin] IS NOT NULL
                            {((region == null || region.IsTotal) ? string.Empty : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
                            AND ((h.[Date Begin] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}'))";
        }

        void AddFreeCars(List<ItemInfo> carBusinesses)
        {
            foreach (var car in App.Cars)
            {
                if (carBusinesses.Any(b => b.ID.Equals(car.ID)))
                    continue;
                else
                {
                    var cb = new ItemInfo();
                    cb.ID = car.ID;
                    cb.Name = car.FullName;
                    cb.Monthes = new Month[0];
                    //cb.Add(new Leasing() { }); //пустой экземпляр - костыль для текущей логики
                    carBusinesses.Add(cb);
                }
            }
        }

        #endregion

        static MaintenanceInfo ReadMaintenanceInfo(SqlDataReader reader)
        {
            var mi = new MaintenanceInfo();
            mi.DateStart = (DateTime)reader["MaintainanceStartDate"];
            mi.DateStart = mi.DateStart.Add(((DateTime)reader["MaintainanceStartTime"]).TimeOfDay);
            mi.DateEnd = (DateTime)reader["MaintainanceEndDate"];
            mi.DateEnd = mi.DateEnd.Add(((DateTime)reader["MaintainanceEndTime"]).TimeOfDay);
            mi.Description = (string)reader["MaintainanceDescription"];

            return mi;
        }

        static bool IsMaintaining(SqlDataReader reader)
        {
            var value = reader["IsMaintaining"] as byte?;

            return value == null ? false : value > 0;
        }

        
    }
}
