using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public L2PAPIClient.DataModel.L2PWhatsNewDataType data;

        public ExtendedWhatsNewData(L2PAPIClient.DataModel.L2PWhatsNewDataType data)
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
    }

    public class DataManager
    {
        private static List<ExtendedWhatsNewData> newStuff = new List<ExtendedWhatsNewData>();
        private static Object myLock = new object();
        private static ILoggingAdapter Logger = new NoneLogger();
        private static int cidCount = 0;
        private static IGetDataProgressCallBack ProgressCallback = null;
        private static Dictionary<string, string> CIDMappings = new Dictionary<string, string>();

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
        

        private static void addStuff(L2PAPIClient.DataModel.L2PWhatsNewDataType data, string cid)
        {
            lock (myLock)
            {
                ExtendedWhatsNewData d = new ExtendedWhatsNewData(data);
                d.cid = cid;
                newStuff.Add(d);
            }
            ProgressCallback.onProgress();
            
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        private static bool isFull()
        {
            bool result;
            lock (myLock)
            {
                result = (newStuff.Count >= cidCount);
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
        private static async void getContentForCID(string cid)
        {
            try
            {
                // Get data from L2P
                var result = await L2PAPIClient.api.Calls.L2PwhatsNewAsync(cid);
#if DEBUG
                Logger.Log("Got Data for: " + cid);
#endif
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
                Logger.Log("Error: No Internet Connection? " + ex.Message);
                // Error? reduce target number of entries
                reportError();
            }
            // finished?
            if (isFull())
            {
                if (ProgressCallback != null)
                {
                    ProgressCallback.onCompleted();
                }

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
                newStuff.Clear();
                ProgressCallback = callback;
#if DEBUG
                Logger.Log("starting Test");
#endif
                // get List of Course Rooms
                var courses = await L2PAPIClient.api.Calls.L2PviewAllCourseInfoByCurrentSemester();
                IEnumerable<string> cids = courses.dataset.Select((x) => x.uniqueid);
                foreach (var item in courses.dataset)
                {
                    CIDMappings.Add(item.uniqueid, item.courseTitle);
                }

#if DEBUG
                Logger.Log("Got Courses");
#endif

                ProgressCallback.beforeGettingCourses(cids.Count());
                // Use Tasks for Multithreading
                var threads = new Task[cids.Count()];
                int i = 0;
                // no getter at the moment, so no need for locking
                cidCount = cids.Count();
                //Parallel.ForEach(string cid in cids)
                foreach (string cid in cids)
                //Parallel.ForEach(cids, (cid) =>
                {

                    threads[i++] = Task.Factory.StartNew(() => getContentForCID(cid));
                }
                
            }
            catch (Exception ex)
            {
                Logger.ImportantLog("Error occured: " + ex.GetType().ToString() + " -- " + ex.Message);
            }
        }

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
            if (data.data.assignements != null && data.data.assignements.Count > 0) result += "  Assignments: " + data.data.assignements.Count + Environment.NewLine;
            if (data.data.discussionItems != null && data.data.discussionItems.Count > 0) result += "  Discussions: " + data.data.discussionItems.Count + Environment.NewLine;
            if (data.data.emails != null && data.data.emails.Count > 0) result += "  Emails: " + data.data.emails.Count + Environment.NewLine;
            if (data.data.hyperlinks != null && data.data.hyperlinks.Count > 0) result += "  Hyperlinks: " + data.data.hyperlinks.Count + Environment.NewLine;
            if (data.data.learningMaterials != null && data.data.learningMaterials.Count > 0) result += "  LearningMaterials: " + data.data.learningMaterials.Count + Environment.NewLine;
            if (data.data.literature != null && data.data.literature.Count > 0) result += "  Literature: " + data.data.literature.Count + Environment.NewLine;
            if (data.data.mediaLibraries != null && data.data.mediaLibraries.Count > 0) result += "  Media: " + data.data.mediaLibraries.Count + Environment.NewLine;
            if (data.data.sharedDocuments != null && data.data.sharedDocuments.Count > 0) result += "  SharedDocs: " + data.data.sharedDocuments.Count + Environment.NewLine;
            return result;
        }
    }
}
