﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Drawing;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class GCGlobal
    {
        public static CLocationRef myLocation = new CLocationRef();

        public static List<CLocationRef> recentLocations = new List<CLocationRef>();

        public static GregorianDateTime dateTimeShown = new GregorianDateTime();

        public static GregorianDateTime dateTimeToday = new GregorianDateTime();

        public static GregorianDateTime dateTimeTomorrow = new GregorianDateTime();

        public static GregorianDateTime dateTimeYesterday = new GregorianDateTime();

        public static TLangFileList languagesList;

        public static String[] applicationStrings = new string[32];

        public static string dialogLastRatedSpec = string.Empty;


        public static string ConfigFolder
        {
            get
            {
                return applicationStrings[GlobalStringsEnum.GSTR_CONFOLDER];
            }
        }

        public static int initFolders()
        {
            string pszBuffer = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            applicationStrings[GlobalStringsEnum.GSTR_APPFOLDER] = pszBuffer;

            applicationStrings[GlobalStringsEnum.GSTR_CONFOLDER] = Path.Combine(applicationStrings[GlobalStringsEnum.GSTR_APPFOLDER], "config");
            Directory.CreateDirectory(applicationStrings[GlobalStringsEnum.GSTR_CONFOLDER]);

            applicationStrings[GlobalStringsEnum.GSTR_LANFOLDER] = Path.Combine(applicationStrings[GlobalStringsEnum.GSTR_APPFOLDER], "lang");
            Directory.CreateDirectory(applicationStrings[GlobalStringsEnum.GSTR_LANFOLDER]);

            applicationStrings[GlobalStringsEnum.GSTR_TEMFOLDER] = Path.Combine(applicationStrings[GlobalStringsEnum.GSTR_APPFOLDER], "temp");
            Directory.CreateDirectory(applicationStrings[GlobalStringsEnum.GSTR_TEMFOLDER]);

            string confDir = applicationStrings[GlobalStringsEnum.GSTR_CONFOLDER];

            applicationStrings[GlobalStringsEnum.GSTR_CE_FILE] = Path.Combine(confDir, "cevents.cfg");
            applicationStrings[GlobalStringsEnum.GSTR_CONF_FILE] = Path.Combine(confDir, "current.cfg");
            applicationStrings[GlobalStringsEnum.GSTR_LOC_FILE] = Path.Combine(confDir, "locations.cfg");
            applicationStrings[GlobalStringsEnum.GSTR_SSET_FILE] = Path.Combine(confDir, "showset.cfg");
            applicationStrings[GlobalStringsEnum.GSTR_LOCX_FILE] = Path.Combine(confDir, "gcal_locations3.rl");//GCAL 3.0
            applicationStrings[GlobalStringsEnum.GSTR_CEX_FILE] = Path.Combine(confDir, "gcal_events3.rl");//GCAL 3.0
            applicationStrings[GlobalStringsEnum.GSTR_CONFX_FILE] = Path.Combine(confDir, "gcal_settings2.rl");
            applicationStrings[GlobalStringsEnum.GSTR_TZ_FILE] = Path.Combine(confDir, "gcal_timezones1.rl");
            applicationStrings[GlobalStringsEnum.GSTR_COUNTRY_FILE] = Path.Combine(confDir, "gcal_countries1.rl");
            applicationStrings[GlobalStringsEnum.GSTR_TEXT_FILE] = Path.Combine(confDir, "gcal_strings2.rl");
            applicationStrings[GlobalStringsEnum.GSTR_TIPS_FILE] = Path.Combine(confDir, "gcal_tips1.txt");
            applicationStrings[GlobalStringsEnum.GSTR_HELP_FILE] = Path.Combine(applicationStrings[GlobalStringsEnum.GSTR_APPFOLDER], "gcal.chm");

            return 1;
        }

        public static string getFileName(int n)
        {
            return applicationStrings[n];
        }


        public static void OpenFile(string fileName)
        {
            GCRichFileLine rf = new GCRichFileLine();
            Rectangle rc;
            recentLocations.Clear();
            if (!File.Exists(fileName))
                return;
            using(StreamReader sr = new StreamReader(fileName))
            {
                // nacitava
                while (rf.SetLine(sr.ReadLine()))
                {
                    switch (rf.TagInt)
                    {
                        case 12701:
                            myLocation.locationName = rf.GetField(0);
                            myLocation.longitudeDeg = double.Parse(rf.GetField(1));
                            myLocation.latitudeDeg = double.Parse(rf.GetField(2));
                            myLocation.offsetUtcHours = double.Parse(rf.GetField(3));
                            myLocation.timeZoneName = rf.GetField(4);
                            myLocation.timezoneId = TTimeZone.GetID(rf.GetField(4));
                            break;
                        case 12702:
                            {
                                CLocationRef loc = new CLocationRef();
                                loc.locationName = rf.GetField(0);
                                loc.longitudeDeg = double.Parse(rf.GetField(1));
                                loc.latitudeDeg = double.Parse(rf.GetField(2));
                                loc.offsetUtcHours = double.Parse(rf.GetField(3));
                                loc.timeZoneName = rf.GetField(4);
                                loc.timezoneId = TTimeZone.GetID(rf.GetField(4));

                                recentLocations.Add(loc);
                            }
                            break;
                        case 12710:
                            rc = new Rectangle();
                            string rcs = String.Format("{0}|{1}|{2}|{3}", rf.GetField(0), rf.GetField(1),
                                rf.GetField(2), rf.GetField(3));
                            if (GCUserInterface.windowController != null)
                                GCUserInterface.windowController.ExecuteMessage("setMainRectangle", new GSString(rcs));
                            break;
                        case 12711:
                            GCDisplaySettings.setValue(int.Parse(rf.GetField(0)), int.Parse(rf.GetField(1)));
                            break;
                        case 12800:
                            GCUserInterface.ShowMode = int.Parse(rf.GetField(0));
                            break;
                        case 12802:
                            GCLayoutData.LayoutSizeIndex = int.Parse(rf.GetField(0));
                            break;
                        case 12900:
                            dialogLastRatedSpec = rf.GetField(0);
                            break;
                        default:
                            break;
                    }
                }
            }

        }


        public static void SaveFile(string fileName)
        {
            GSCore rcs = GCUserInterface.windowController.ExecuteMessage("getMainRectangle", (GSCore)null);

            using(StreamWriter f = new StreamWriter(fileName))
            {
                f.WriteLine("12701 {0}|{1}|{2}|{3}|{4}", myLocation.locationName, myLocation.longitudeDeg, myLocation.latitudeDeg,
                    myLocation.offsetUtcHours, TTimeZone.GetTimeZoneName(myLocation.timezoneId));
                foreach (CLocationRef loc in recentLocations)
                {
                    f.WriteLine("12702 {0}|{1}|{2}|{3}|{4}", loc.locationName,
                        loc.longitudeDeg, loc.latitudeDeg,
                        loc.offsetUtcHours, TTimeZone.GetTimeZoneName(loc.timezoneId));
                }
                f.WriteLine("12710 {0}", rcs.getStringValue());
                for (int y = 0; y < GCDisplaySettings.getCount(); y++)
                {
                    f.WriteLine("12711 {0}|{1}", y, GCDisplaySettings.getValue(y));
                }
                f.WriteLine("12800 {0}", GCUserInterface.ShowMode);
                f.WriteLine("12802 {0}", GCLayoutData.LayoutSizeIndex);
                f.WriteLine("12900 {0}", dialogLastRatedSpec);
            }
        }


        public static bool GetLangFileForAcr(string pszAcr, out string strFile)
        {
            foreach (TLangFileInfo p in languagesList.list)
            {
                if (p.m_strAcr.Equals(pszAcr, StringComparison.CurrentCultureIgnoreCase))
                {
                    strFile = p.m_strFile;
                    return true;
                }
            }
            strFile = null;
            return false;
        }


        public static void LoadInstanceData()
        {
            // initialization for AppDir
            initFolders();

            // initialization of global strings
            GCStrings.readFile(getFileName(GlobalStringsEnum.GSTR_TEXT_FILE));

            // inicializacia timezones
            TTimeZone.OpenFile(getFileName(GlobalStringsEnum.GSTR_TZ_FILE));

            // inicializacia countries
            TCountry.InitWithFile(getFileName(GlobalStringsEnum.GSTR_COUNTRY_FILE));

            // inicializacia miest a kontinentov
            CLocationList.OpenFile(getFileName(GlobalStringsEnum.GSTR_LOCX_FILE));

            // inicializacia zobrazovanych nastaveni
            GCDisplaySettings.readFile(getFileName(GlobalStringsEnum.GSTR_SSET_FILE));

            // inicializacia custom events
            GCFestivalBookCollection.OpenFile(getFileName(GlobalStringsEnum.GSTR_CONFOLDER));

            // looking for files *.recn
            GCConfigRatedManager.RefreshListFromDirectory(getFileName(GlobalStringsEnum.GSTR_CONFOLDER));

            // initialization of global variables
            myLocation.EncodedString = CLocationRef.DefaultEncodedString;

            recentLocations.Add(new CLocationRef(myLocation));

            OpenFile(getFileName(GlobalStringsEnum.GSTR_CONFX_FILE));
            // refresh fasting style after loading user settings
            //GCFestivalBook.SetOldStyleFasting(GCDisplaySettings.getValue(42));

            // inicializacia tipov dna
            if (!File.Exists(getFileName(GlobalStringsEnum.GSTR_TIPS_FILE)))
            {
                File.WriteAllText(getFileName(GlobalStringsEnum.GSTR_TIPS_FILE), Properties.Resources.tips);
            }
        }


        public static void SaveInstanceData()
        {
            SaveFile(getFileName(GlobalStringsEnum.GSTR_CONFX_FILE));

            if (CLocationList.IsModified())
            {
                CLocationList.SaveAs(getFileName(GlobalStringsEnum.GSTR_LOCX_FILE), 4);//GCAL 3.0
            }

            if (TCountry.IsModified())
            {
                TCountry.SaveToFile(getFileName(GlobalStringsEnum.GSTR_COUNTRY_FILE));
            }

            if (GCStrings.gstr_Modified)
            {
                GCStrings.writeFile(getFileName(GlobalStringsEnum.GSTR_TEXT_FILE));
            }

            GCDisplaySettings.writeFile(getFileName(GlobalStringsEnum.GSTR_SSET_FILE));

            GCFestivalBookCollection.SaveFile(getFileName(GlobalStringsEnum.GSTR_CONFOLDER));

            if (TTimeZone.Modified)
            {
                TTimeZone.SaveFile(getFileName(GlobalStringsEnum.GSTR_TZ_FILE));
            }

        }


        public static void AddRecentLocation(CLocationRef cLocationRef)
        {
            int rl = recentLocations.IndexOf(cLocationRef);
            if (rl != 0)
            {
                if (rl > 0)
                {
                    recentLocations.RemoveAt(rl);
                }
                recentLocations.Insert(0, cLocationRef);
            }
        }

        public static CLocationRef LastLocation
        {
            get
            {
                if (recentLocations.Count == 0)
                    return myLocation;
                return recentLocations[0];
            }
        }
    }
}