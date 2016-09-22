using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Xamarin.Forms;

namespace L2PNewsTicker
{
    /// <summary>
    /// Interface to enable the injection of Logging for the DataManager
    /// </summary>
    public interface ILoggingAdapter
    {
        void Log(string text);
        void ImportantLog(string text);
    }

    /// <summary>
    /// Extension of the WhatsNew Data of L2P to contain also the cid information
    /// </summary>
    public class ExtendedWhatsNewData
    {
        public string cid;
        public L2PAPIClientPortable.DataModel.L2PWhatsNewDataType data;

        public ExtendedWhatsNewData(L2PAPIClientPortable.DataModel.L2PWhatsNewDataType data)
        {
            this.data = data;
        }
    }

    public interface IGetDataProgressCallBack
    {
        /// <summary>
        /// Called after finished work
        /// </summary>
        void onCompleted();
        /// <summary>
        /// Called before getting Course Data, but already having called CourseInfo
        /// </summary>
        /// <param name="cids">Number of cids to work with</param>
        void beforeGettingCourses(int cids);
        /// <summary>
        /// Called each time a DataCall is finished
        /// </summary>
        void onProgress();
        /*/// <summary>
        /// Called when the operation was aborted
        /// </summary>
        void onCancel();*/
    }

    public class CourseCellAdapter
    {
        public string Text { get; set; }
        public string Detail { get; set; }
        public string cid { get; set; }
        public Xamarin.Forms.Color MainColor
        {
            get
            {
                return MainPage.FontColor;
            }
        }
        public Xamarin.Forms.Color DetailColor
        {
            get
            {
                return MainPage.FontColor;
            }
        }
    }

    public class DataManager
    {
        private static List<ExtendedWhatsNewData> newStuff = new List<ExtendedWhatsNewData>();
        private static List<ExtendedWhatsNewData> tmpStuff = new List<ExtendedWhatsNewData>();
        private static Object myLock = new object();
        private static ILoggingAdapter Logger = new NoneLogger();
        private static int cidCount = 0;
        private static IGetDataProgressCallBack ProgressCallback = null;
        private static Dictionary<string, string> CIDMappings = new Dictionary<string, string>();

        public static bool hasData()
        {
            return (newStuff.Count > 0);
        }

        public static void StoreAsTask()
        {
            Task t = new Task(() => Store());
            t.Start();
        }

        public static void Store()
        {
            try
            {
                string newStuffJSON = Newtonsoft.Json.JsonConvert.SerializeObject(newStuff);
                string MappingsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(CIDMappings);

                Application.Current.Properties["newStuff"] = newStuffJSON;
                Application.Current.Properties["CIDMappings"] = MappingsJSON;

#if WINDOWS_PHONE
            // nothing
#else
                Application.Current.SavePropertiesAsync();
#endif
            }
            catch
            {
                // Typcial Exception: Invalid Acces, because Collection is updated during json-conversion.
                // For now, just try again
                StoreAsTask();
            }
        }

        public static bool Load()
        {
            object o;
            if (!Application.Current.Properties.TryGetValue("newStuff",out o))
            {
                // no data
                return false;
            }
            string json = (string)o;
            newStuff = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExtendedWhatsNewData>>(json);

            object p;
            if (!Application.Current.Properties.TryGetValue("CIDMappings", out p))
            {
                // no data
                return false;
            }
            json = (string)p;
            CIDMappings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return true;
        }

        public static void setLogger(ILoggingAdapter l)
        {
            Logger = l;
        }

        /// <summary>
        /// A trivial Logger to avoid null pointers
        /// </summary>
        public class NoneLogger : ILoggingAdapter
        {
            public void ImportantLog(string text) { }
            public void Log(string text) { }
        }
        
        /// <summary>
        /// Adds more Stuff to tmpStuff
        /// </summary>
        private static void addStuff(L2PAPIClientPortable.DataModel.L2PWhatsNewDataType data, string cid)
        {
            lock (myLock)
            {
                ExtendedWhatsNewData d = new ExtendedWhatsNewData(data);
                d.cid = cid;
                //newStuff.Add(d);
                tmpStuff.Add(d);
            }
            ProgressCallback.onProgress();
            
        }

        private static void addStuff(IEnumerable<L2PAPIClientPortable.DataModel.L2PWhatsNewExtendedDataType> list)
        {
            lock(myLock)
            {
                int i = 0;
                foreach (var item in list)
                {
                    ExtendedWhatsNewData d = new ExtendedWhatsNewData(item);
                    d.cid = item.cid;
                    tmpStuff.Add(d);
                }
                /*for (i=0;i<list.Count();i++)
                {
                    var data = list.ElementAt(i);
                    ExtendedWhatsNewData d = new ExtendedWhatsNewData(data);
                    d.cid = cid;
                    //newStuff.Add(d);
                    tmpStuff.Add(d);
                }*/
                ProgressCallback.onProgress();
            }

        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        private static bool isFull()
        {
            bool result;
            lock (myLock)
            {
                //result = (newStuff.Count >= cidCount);
                result = (tmpStuff.Count >= cidCount);
            }
            return result;
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        private static void reportError()
        {
            lock (myLock)
            {
                cidCount--;
            }
            ProgressCallback.onProgress();
        }

        /// <summary>
        /// Gets the WhatsNew-Content for the given CID
        /// </summary>
        private static async void getContentForCID(string cid, CancellationToken ct)
        {
            // Simple idea: Cancelled? Report for data source size, then return
            if (ct != null && ct.IsCancellationRequested)
            {
#if DEBUG
                Logger.Log("Data for " + cid + " aborted");
#endif
                return;
            }
            try
            {
                // Get data from L2P
                //var result = await L2PAPIClient.api.Calls.L2PwhatsNewAsync(cid);
                int since;
                object sinceObject;
                if (!Application.Current.Properties.TryGetValue("since", out sinceObject))
                {
                    since = 60 * 24; // Fallback: 24 hours
                }
                else
                {
                    since = (int)sinceObject;
                }
                var result = await L2PAPIClientPortable.api.Calls.L2PwhatsNewSinceAsync(cid, since);
//#if DEBUG
                //Logger.Log("Got Data for: " + cid);
                Logger.Log(cid+ " ✓");
//#endif
                // If cancelled while getting data, return to avoid data inconsistencies
                if (ct != null && ct.IsCancellationRequested)
                {
#if DEBUG
                    Logger.Log("Data for " + cid+" aborted");
#endif
                    return;
                }
                // Add to Data Management
                if (result.status)
                    addStuff(result,cid);
            }
#if (__ANDROID__ || __IOS__)
            catch (System.Net.WebException ex)
#else
                    catch (System.Net.Http.HttpRequestException ex)
#endif
            {
                //Logger.Log(Localization.Localize("NoInternet"));
                Logger.Log(cid + " × " + "(Internet Error)");
                // Error? reduce target number of entries
                reportError();
            }
            catch (Exception)
            {
                Logger.Log(cid + " x " + "(Server/Internet Error)");
                reportError();
            }
            // finished?
            if (isFull())
            {
                if (ProgressCallback != null)
                {
                    newStuff.Clear();
                    newStuff.AddRange(tmpStuff);
                    //newStuff = tmpStuff;
                    ProgressCallback.onCompleted();
                }

            }
        }

        private static async void getContentBySemester(CancellationToken ct, bool oldCourses=false)
        {
            // Simple idea: Cancelled? Report for data source size, then return
            if (ct != null && ct.IsCancellationRequested)
            {
#if DEBUG
                Logger.Log("Data aborted");
#endif
                return;
            }
            try
            {
                // Get data from L2P
                //var result = await L2PAPIClient.api.Calls.L2PwhatsNewAsync(cid);
                int since;
                object sinceObject;
                if (!Application.Current.Properties.TryGetValue("since", out sinceObject))
                {
                    since = 60 * 24; // Fallback: 24 hours
                }
                else
                {
                    since = (int)sinceObject;
                }
                //var result = await L2PAPIClientPortable.api.Calls.L2PwhatsNewSinceAsync(cid, since);
                L2PAPIClientPortable.DataModel.L2PWhatsAllNewDataType result;
                if (oldCourses)
                {
                    result = await L2PAPIClientPortable.api.Calls.L2PwhatsAllNewSinceForSemesterAsync(GetLastSemesterString(), since);
                }
                else
                {
                    result = await L2PAPIClientPortable.api.Calls.L2PwhatsAllNewSinceAsync(since);
                }
                //#if DEBUG
                //Logger.Log("Got Data for: " + cid);
                if (oldCourses)
                    Logger.Log("last semester ✓");
                else
                    Logger.Log("current semester ✓");
                //#endif
                // If cancelled while getting data, return to avoid data inconsistencies
                if (ct != null && ct.IsCancellationRequested)
                {
#if DEBUG
                    Logger.Log("Data aborted");
#endif
                    return;
                }
                // Add to Data Management (one by one)
                /*foreach (var item in result.dataset)
                {
                    if (item.status)
                        addStuff(item, item.cid);
                }*/

                if (result.status)
                {
                    var filterList = result.dataset.FindAll((x) => x.status);
                    addStuff(filterList);
                }

                
            }
#if (__ANDROID__ || __IOS__)
            catch (System.Net.WebException ex)
#else
                    catch (System.Net.Http.HttpRequestException ex)
#endif
            {
                //Logger.Log(Localization.Localize("NoInternet"));
                Logger.Log(" × " + "(Internet Error)");
                // Error? reduce target number of entries
                reportError();
            }
            catch (Exception)
            {
                Logger.Log(" x " + "(Server/Internet Error)");
                reportError();
            }
            // finished?
            if (isFull())
            {
                if (ProgressCallback != null)
                {
                    newStuff.Clear();
                    newStuff.AddRange(tmpStuff);
                    //newStuff = tmpStuff;
                    ProgressCallback.onCompleted();
                }

            }
        }

        /// <summary>
        /// This flag is used for signalling abortion between getting courses and course updates
        /// </summary>
        private static bool abortFlag = false;

        /// <summary>
        /// making the Tokens accessible outside the method allows aborting the work
        /// </summary>
        //private static Task[] threads;
        private static CancellationTokenSource[] cancelTokens;

        public static void CancelUpdate()
        {
            //set flag for in-between calls case
            abortFlag = true;
            if (cancelTokens != null)
            {
                // Try/catch in here for the case of bad access during enumeration
                try
                {
                    foreach (var item in cancelTokens)
                    {
                        try
                        {
                            item.Cancel();
                        }
                        catch { }
                        finally
                        {
                            // If Object, Dispose it
                            if (item != null)
                                item.Dispose();
                        }
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Start updating the current Data of whatsNew
        /// </summary>
        /// <param name="callback">An (optional) Callback after the Data has been fetched completely</param>
        public static async Task startUpdate(IGetDataProgressCallBack callback = null)
        {
            try
            {
                //newStuff.Clear();
                tmpStuff.Clear();
                ProgressCallback = callback;
//#if DEBUG
                Logger.Log(Localization.Localize("Starting"));
                //#endif
                List<string> cids = new List<string>();

                if (UseOldCourses)
                {
                    // get List of Course Rooms
                    var courses = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                    var courses2 = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseIfoBySemesterAsync(GetLastSemesterString());
                    // In case of no Courses in the dataSet, inform caller by using an exception (No Callback-Action!)
                    if (courses.dataset == null || courses.dataset.Count == 0)
                    {
                        if (courses2.dataset == null || courses2.dataset.Count == 0)
                            throw new NoCoursesException();
                    }
                    cids.AddRange(courses.dataset.Select((x) => x.uniqueid));
                    cids.AddRange(courses2.dataset.Select((x) => x.uniqueid));

                    foreach (var item in courses.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                    foreach (var item in courses2.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                }
                else
                {

                    // get List of Course Rooms
                    var courses = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                    // In case of no Courses in the dataSet, inform caller by using an exception (No Callback-Action!)
                    if (courses.dataset == null || courses.dataset.Count == 0) throw new NoCoursesException();
                    cids.AddRange(courses.dataset.Select((x) => x.uniqueid));
                    foreach (var item in courses.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                }

//#if DEBUG
                Logger.Log(Localization.Localize("GotCourses"));
//#endif

                ProgressCallback.beforeGettingCourses(cids.Count());
                // Use Tasks for Multithreading
                var threads = new Task[cids.Count()];
                cancelTokens = new CancellationTokenSource[cids.Count()];
                int i = 0;
                // no getter at the moment, so no need for locking
                cidCount = cids.Count();
                //Parallel.ForEach(string cid in cids)
                foreach (string cid in cids)
                //Parallel.ForEach(cids, (cid) =>
                {
                    var TokenSource = new CancellationTokenSource();
                    // Save Token to enable cancallation later on
                    cancelTokens[i] = TokenSource;
                    var token = TokenSource.Token;
                    threads[i] = Task.Factory.StartNew(() => getContentForCID(cid, token), token);
                    i++;
                }
                
            }
            /*catch (AggregateException ex)
            {
                // Idea: If Inner Exception is 
                if (ex.InnerException is L2PAPIClient.AuthenticationManager.NotAuthorizedException)
                {
                    throw ex.InnerException;
                }
            }*/
            catch (L2PAPIClientPortable.AuthenticationManager.NotAuthorizedException ex)
            {
                // Authorization problem - throw further
                throw ex;

            }
        }

        public static async Task startUpdateNew(IGetDataProgressCallBack callback = null)
        {
            try
            {
                //newStuff.Clear();
                tmpStuff.Clear();
                ProgressCallback = callback;
                //#if DEBUG
                Logger.Log(Localization.Localize("Starting"));
                //#endif
                List<string> cids = new List<string>();

                if (UseOldCourses)
                {
                    // get List of Course Rooms
                    var courses = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                    var courses2 = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseIfoBySemesterAsync(GetLastSemesterString());
                    // In case of no Courses in the dataSet, inform caller by using an exception (No Callback-Action!)
                    if (courses.dataset == null || courses.dataset.Count == 0)
                    {
                        if (courses2.dataset == null || courses2.dataset.Count == 0)
                            throw new NoCoursesException();
                    }
                    cids.AddRange(courses.dataset.Select((x) => x.uniqueid));
                    cids.AddRange(courses2.dataset.Select((x) => x.uniqueid));

                    foreach (var item in courses.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                    foreach (var item in courses2.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                }
                else
                {

                    // get List of Course Rooms
                    var courses = await L2PAPIClientPortable.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                    // In case of no Courses in the dataSet, inform caller by using an exception (No Callback-Action!)
                    if (courses.dataset == null || courses.dataset.Count == 0) throw new NoCoursesException();
                    cids.AddRange(courses.dataset.Select((x) => x.uniqueid));
                    foreach (var item in courses.dataset)
                    {
                        CIDMappings[item.uniqueid] = item.courseTitle;
                        //CIDMappings.Add(item.uniqueid, item.courseTitle);
                    }
                }

                //#if DEBUG
                Logger.Log(Localization.Localize("GotCourses"));
                //#endif

                int cidCounts;
                if (UseOldCourses)
                    cidCounts = 2;
                else
                    cidCounts = 1;

                ProgressCallback.beforeGettingCourses(cidCounts);
                // Use Tasks for Multithreading
                var threads = new Task[2];
                cancelTokens = new CancellationTokenSource[2];
                // no getter at the moment, so no need for locking
                cidCount = cids.Count();
                //Parallel.ForEach(string cid in cids)
                /*foreach (string cid in cids)
                //Parallel.ForEach(cids, (cid) =>
                {
                    var TokenSource = new CancellationTokenSource();
                    // Save Token to enable cancallation later on
                    cancelTokens[i] = TokenSource;
                    var token = TokenSource.Token;
                    threads[i] = Task.Factory.StartNew(() => getContentForCID(cid, token), token);
                    i++;
                }*/

                var TokenSource = new CancellationTokenSource();
                // Save Token to enable cancallation later on
                cancelTokens[0] = TokenSource;
                var token = TokenSource.Token;
                threads[0] = Task.Factory.StartNew(() => getContentBySemester(token));

                if (UseOldCourses)
                {
                    TokenSource = new CancellationTokenSource();
                    // Save Token to enable cancallation later on
                    cancelTokens[1] = TokenSource;
                    token = TokenSource.Token;
                    threads[1] = Task.Factory.StartNew(() => getContentBySemester(token,true));
                }

            }
            /*catch (AggregateException ex)
            {
                // Idea: If Inner Exception is 
                if (ex.InnerException is L2PAPIClient.AuthenticationManager.NotAuthorizedException)
                {
                    throw ex.InnerException;
                }
            }*/
            catch (L2PAPIClientPortable.AuthenticationManager.NotAuthorizedException ex)
            {
                // Authorization problem - throw further
                throw ex;

            }
        }

        public class NoCoursesException : Exception { }

        /// <summary>
        /// Use the Logger to show the content in the data (very raw - only for debug)
        /// </summary>
        public static void ShowUpdatesViaLog()
        {
            foreach (var data in newStuff)
            {
                Logger.Log(GetStringFromRoomData(data));
            }
        }

        public static List<string> GetUpdateStringsAsList()
        {
            List<string> list = new List<string>();
            foreach (var data in newStuff)
            {
                // add only courses with news
                string s = GetStringFromRoomData(data);
                s = s.Trim();
                if (s.Contains(Environment.NewLine))
                    list.Add(s);
            }
            return list;
        }

        private static string GetStringFromRoomData(ExtendedWhatsNewData data)
        {
            string result = "For CID " + data.cid + " ( "+CIDMappings[data.cid]+" ) :" + Environment.NewLine;
            if (data.data.wikis != null && data.data.wikis.Count>0) result += "  Wikis: " + data.data.wikis.Count + Environment.NewLine;
            if (data.data.announcements != null && data.data.announcements.Count > 0) result += "  Announcements: " + data.data.announcements.Count + Environment.NewLine;
            if (data.data.assignments != null && data.data.assignments.Count > 0) result += "  Assignments: " + data.data.assignments.Count + Environment.NewLine;
            if (data.data.discussionItems != null && data.data.discussionItems.Count > 0) result += "  Discussions: " + data.data.discussionItems.Count + Environment.NewLine;
            if (data.data.emails != null && data.data.emails.Count > 0) result += "  Emails: " + data.data.emails.Count + Environment.NewLine;
            if (data.data.hyperlinks != null && data.data.hyperlinks.Count > 0) result += "  Hyperlinks: " + data.data.hyperlinks.Count + Environment.NewLine;
            if (data.data.learningMaterials != null && data.data.learningMaterials.Count > 0) result += "  LearningMaterials: " + data.data.learningMaterials.Count + Environment.NewLine;
            if (data.data.literature != null && data.data.literature.Count > 0) result += "  Literature: " + data.data.literature.Count + Environment.NewLine;
            if (data.data.mediaLibraries != null && data.data.mediaLibraries.Count > 0) result += "  Media: " + data.data.mediaLibraries.Count + Environment.NewLine;
            if (data.data.sharedDocuments != null && data.data.sharedDocuments.Count > 0) result += "  SharedDocs: " + data.data.sharedDocuments.Count + Environment.NewLine;
            return result;
        }

        public static IEnumerable<CourseCellAdapter> GetCoursesCellAdaption()
        {
            List<CourseCellAdapter> cellData = new List<CourseCellAdapter>();

            // Traverse elements and add them
            foreach (var data in newStuff)
            {
                CourseCellAdapter cell = new CourseCellAdapter();
                // Use humn-readable name
                cell.Text = CIDMappings[data.cid];
                // For detail traverse whole dataset
                string result = "";
                if (data.data.wikis != null && data.data.wikis.Count > 0) result += "  Wikis: " + data.data.wikis.Count + Environment.NewLine;
                if (data.data.announcements != null && data.data.announcements.Count > 0) result += "  "+ Localization.Localize("Announcements")+": " + data.data.announcements.Count + Environment.NewLine;
                if (data.data.assignments != null && data.data.assignments.Count > 0) result += "  "+ Localization.Localize("Assignments")+": " + data.data.assignments.Count + Environment.NewLine;
                if (data.data.discussionItems != null && data.data.discussionItems.Count > 0) result += "  "+ Localization.Localize("Discussions")+": " + data.data.discussionItems.Count + Environment.NewLine;
                if (data.data.emails != null && data.data.emails.Count > 0) result += "  Emails: " + data.data.emails.Count + Environment.NewLine;
                if (data.data.hyperlinks != null && data.data.hyperlinks.Count > 0) result += "  Hyperlinks: " + data.data.hyperlinks.Count + Environment.NewLine;
                if (data.data.learningMaterials != null && data.data.learningMaterials.Count > 0) result += "  "+ Localization.Localize("LearningMaterials")+": " + data.data.learningMaterials.Count + Environment.NewLine;
                if (data.data.literature != null && data.data.literature.Count > 0) result += "  "+ Localization.Localize("Literature")+": " + data.data.literature.Count + Environment.NewLine;
                if (data.data.mediaLibraries != null && data.data.mediaLibraries.Count > 0) result += "  "+ Localization.Localize("MediaLibraries")+": " + data.data.mediaLibraries.Count + Environment.NewLine;
                if (data.data.sharedDocuments != null && data.data.sharedDocuments.Count > 0) result += "  "+ Localization.Localize("SharedDocuments")+": " + data.data.sharedDocuments.Count + Environment.NewLine;
                // If empty, just inform user
                if (result == "")
                    result = Localization.Localize("NothingNew");
                cell.Detail = result;
                cell.cid = data.cid;
                cellData.Add(cell);
            }

            cellData.Sort((x, y) => String.Compare(x.cid, y.cid));
            return cellData;
        }

        /// <summary>
        /// Returns the new elements from local storage for provided CID
        /// Will return null for missing values
        /// </summary>
        public static L2PAPIClientPortable.DataModel.L2PWhatsNewDataType GetCourseDataElements(string cid)
        {
            var data = newStuff.Find((x) => x.cid == cid);
            if (data == null) return null; // Avoid Null Pointer Exceptions
            return data.data;
        }

        public static List<object> GetCourseDataElementsFlat(string cid)
        {
            // Get original dataset
            var data = GetCourseDataElements(cid);
            // Check for Null
            if (data == null) return null;

            List<object> flatList = new List<object>();
            if (data.announcements != null) flatList.AddRange(data.announcements);
            if (data.assignments != null) flatList.AddRange(data.assignments);
            if (data.discussionItems != null) flatList.AddRange(data.discussionItems);
            if (data.emails != null) flatList.AddRange(data.emails);
            if (data.hyperlinks != null) flatList.AddRange(data.hyperlinks);
            if (data.learningMaterials != null) flatList.AddRange(data.learningMaterials);
            if (data.literature != null) flatList.AddRange(data.literature);
            if (data.mediaLibraries != null) flatList.AddRange(data.mediaLibraries);
            if (data.sharedDocuments != null) flatList.AddRange(data.sharedDocuments);
            if (data.wikis != null) flatList.AddRange(data.wikis);

            return flatList;
        }

        /// <summary>
        /// Get the term id of the current semester (ws14, etc.)
        /// </summary>
        public static string GetCurrentSemesterString()
        {
            string s = "";
            // Depending on month, decide whether summer or winter term
            if (DateTime.Now.Month < 3 || DateTime.Now.Month > 9)
                s = "ws";
            else
                s = "ss";

            int y = DateTime.Now.Year;
            if (DateTime.Now.Month < 3) y--; // if it is early in year
            string year = y.ToString();

            s = s + year[2] + year[3]; // append the two-digit year to term id

            return s;
        }

        /// <summary>
        /// Get the semester id of the last semester
        /// </summary>
        public static string GetLastSemesterString()
        {
            string current = GetCurrentSemesterString();

            // in winter term, we can simply replace ws by ss and we are finished
            if (current[0] == 'w')
            {
                return current.Replace("ws", "ss");
            }
            // else, we need to decrease the year
            string sub = current.Substring(2);
            int year = int.Parse(sub);
            year--;
            return "ws" + year;

        }
        
        /// <summary>
        /// Property indicating whether the user wants to query also the old courses
        /// </summary>
        public static bool UseOldCourses
        {
            get
            {
                try
                {
                    bool u = (bool)Application.Current.Properties["UseOldCourses"];
                    return u;
                }
                catch
                {
                    Application.Current.Properties["UseOldCourses"] = false;
                    return false;
                }
            }
            set
            {
                Application.Current.Properties["UseOldCourses"] = value;
            }
        }
    }
}
