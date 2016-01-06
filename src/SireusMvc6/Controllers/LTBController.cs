using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using Microsoft.AspNet.Mvc;
using LTBCore;
using SireusMvc6.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;

namespace SireusMvc6.Controllers
{
    public class LtbController : Controller
    {
        private static readonly int[] InstalledBasePerYear = new int[LtbCommon.MaxYear + 1];
        private static readonly double[] FailureRatePerYear = new double[LtbCommon.MaxYear + 1];
        private static readonly double[] RepairLossPerYear = new double[LtbCommon.MaxYear + 1];
        private static readonly int[] RegionalStocksPerYear = new int[LtbCommon.MaxYear + 1];

        private static string _stock=string.Empty;
        private static string _safety = string.Empty;
        private static string _failed = string.Empty;
        private static string _repaired = string.Empty;
        private static string _lost = string.Empty;
        private static string _total = string.Empty;
        private static MemoryStream _ltbChart;

        private static double _confidenceLevelConverted;
        private static int _leadDays;
        private static int _nbrOfSamples;
        private static double _confidenceLevelFromNormsInv;
        private static string _textInfo;
        private static int _tabidx;
        private static bool _repairSelected;
        // GET: /LTB/

        private static DateTime LifeTimeBuy { get; set; }
        private static DateTime EndOfService { get; set; }


        private FileContentResult ClearedChart
        {
            get
            {
                var ms = LtbCommon.GetEmptyChart();
                return File(ms.GetBuffer(), "image/png");
            }
        }


        private static int ServiceDays
        {
            get
            {
                return Convert.ToInt32(DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, LifeTimeBuy, EndOfService));
            }
        }

        private static int ServiceYears
        {
            get
            {
                if (LifeTimeBuy.Year == EndOfService.Year)
                {
                    return 0;
                }

                var newYear = Convert.ToDateTime(LifeTimeBuy.Year + "-01-01");
                if (IsLeapYear(LifeTimeBuy.Year) &
                    DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, newYear, LifeTimeBuy) < 59)
                {
                    return
                        Convert.ToInt32(
                            (DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, LifeTimeBuy, EndOfService) +
                             CountLeaps(LifeTimeBuy.Year) - CountLeaps(EndOfService.Year) - 2)/365);
                }
                return
                    Convert.ToInt32((DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, LifeTimeBuy, EndOfService) +
                                     CountLeaps(LifeTimeBuy.Year) - CountLeaps(EndOfService.Year) - 1)/365);
            }
        }

        public ActionResult Index()
        {
            CheckErrorAlert();
            SetPageDefaultValues();
            InitConfidenceDropDown_CheckUI();
            return View("Index");
        }


        public ActionResult Calculate(string data)
        {
            var arg = data.Split('!');

            if (arg[0] == "en")
            {
                Calculateen(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45], arg[46], arg[47], arg[48], arg[49]);
            }
            else
            {
                Calculatesv(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45]);
            }
            var ci = new CultureInfo("sv-SE");
            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
            return PartialView("_Result");
        }

        [HttpGet]
        public FileContentResult Chart()
        {
            return (_ltbChart == null) ? ClearedChart : File(_ltbChart.GetBuffer(), "image/png");
        }

        [HttpGet]
        public ActionResult Clear()
        {
            SetPageDefaultValues();
            ViewBag.SetFocus = "IB0";
            CheckErrorAlert();
            return View("Index");
        }


        public ActionResult Repair(string data)
        {
            var arg = data.Split('!');

            if (arg[0] == "en")
            {
                Repairen(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45], arg[46], arg[47], arg[48], arg[49]);
            }
            else
            {
                Repairsv(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45]);
            }
            var ci = new CultureInfo("sv-SE");
            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
            return PartialView("_Ltb2");
        }


        public ActionResult NoRepair(string data)
        {
            var arg = data.Split('!');

            if (arg[0] == "en")
            {
                NoRepairen(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45], arg[46], arg[47], arg[48], arg[49]);
            }
            else
            {
                NoRepairsv(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45]);
            }
            var ci = new CultureInfo("sv-SE");
            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
            return PartialView("_Ltb2");
        }


        public ActionResult LtbDate(string data)
        {
            if (data == null) return PartialView("_Ltb2");
            var arg = data.Split('!');

            if (arg[0] == "en")
            {
                LtbDateen(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45], arg[46], arg[47], arg[48], arg[49]);
            }
            else
            {
                LtbDatesv(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45]);
            }
            return PartialView("_Ltb2");
        }


        public ActionResult EosDate(string data)
        {
            if (data==null) return PartialView("_Ltb2");
            var arg = data.Split('!');

            if (arg[0] == "en")
            {
                EosDateen(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45], arg[46], arg[47], arg[48], arg[49]);
            }
            else
            {
                EosDatesv(arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8], arg[9], arg[10], arg[11],
                    arg[12], arg[13], arg[14], arg[15], arg[16], arg[17], arg[18], arg[19], arg[20], arg[21], arg[22],
                    arg[23], arg[24], arg[25], arg[26], arg[27], arg[28], arg[29], arg[30], arg[31], arg[32], arg[33],
                    arg[34], arg[35], arg[36], arg[37], arg[38], arg[39], arg[40], arg[41], arg[42], arg[43], arg[44],
                    arg[45]);
            }
            return PartialView("_Ltb2");
        }

        private void InitYearTabIndex(bool repairAvailable)
        {
            var yearCnt = 0;
            var nbrOfServiceYears = 0;
            //var a = new Actual();
            var ci = new CultureInfo("sv-SE");
            LifeTimeBuy = Convert.ToDateTime(ViewData["LTBDate"].ToString(),ci);
            EndOfService = Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci);
            ViewBag.Year0 = "LTB";
            nbrOfServiceYears = ServiceYears;
            yearCnt = 0;
            _tabidx = 1;
            while (yearCnt <= nbrOfServiceYears)
            {
                switch (yearCnt)
                {
                    case 0:
                        ViewBag.Year1 = "+1Year";

                        if (ViewData["IB1"] != null)
                        {
                            if (ViewData["IB1"].ToString() == "EoS") ViewData["IB1"] = string.Empty;
                        }
                        ViewData["IB1ForeColor"] = "black40";
                        ViewData["IB0TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR0TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS0TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL0TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB1TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR1TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS1TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL1TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB1\""));
                        ViewData["RS1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS1\""));
                        ViewData["FR1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR1\""));
                        ViewData["RL1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL1\""));
                        break;
                    case 1:
                        ViewBag.Year2 = "+2Year";

                        if (ViewData["IB2"] != null)
                        {
                            if (ViewData["IB2"].ToString() == "EoS") ViewData["IB2"] = string.Empty;
                        }
                        ViewData["IB2ForeColor"] = "black40";
                        ViewData["IB2TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR2TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS2TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL2TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB2\""));
                        ViewData["RS2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS2\""));
                        ViewData["FR2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR2\""));
                        ViewData["RL2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL2\""));
                        break;
                    case 2:
                        ViewBag.Year3 = "+3Year";

                        if (ViewData["IB3"] != null)
                        {
                            if (ViewData["IB3"].ToString() == "EoS") ViewData["IB3"] = string.Empty;
                        }
                        ViewData["IB3ForeColor"] = "black40";
                        ViewData["IB3TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR3TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS3TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL3TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB3\""));
                        ViewData["RS3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS3\""));
                        ViewData["FR3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR3\""));
                        ViewData["RL3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL3\""));
                        break;
                    case 3:
                        ViewBag.Year4 = "+4Year";

                        if (ViewData["IB4"] != null)
                        {
                            if (ViewData["IB4"].ToString() == "EoS") ViewData["IB4"] = string.Empty;
                        }
                        ViewData["IB4ForeColor"] = "black40";
                        ViewData["IB4TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR4TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS4TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL4TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB4\""));
                        ViewData["RS4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS4\""));
                        ViewData["FR4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR4\""));
                        ViewData["RL4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL4\""));
                        break;
                    case 4:
                        ViewBag.Year5 = "+5Year";

                        if (ViewData["IB5"] != null)
                        {
                            if (ViewData["IB5"].ToString() == "EoS") ViewData["IB5"] = string.Empty;
                        }
                        ViewData["IB5ForeColor"] = "black40";
                        ViewData["IB5TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR5TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS5TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL5TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB5\""));
                        ViewData["RS5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS5\""));
                        ViewData["FR5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR5\""));
                        ViewData["RL5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL5\""));
                        break;
                    case 5:
                        ViewBag.Year6 = "+6Year";

                        if (ViewData["IB6"] != null)
                        {
                            if (ViewData["IB6"].ToString() == "EoS") ViewData["IB6"] = string.Empty;
                        }
                        ViewData["IB6ForeColor"] = "black40";
                        ViewData["IB6TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR6TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS6TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL6TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB6\""));
                        ViewData["RS6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS6\""));
                        ViewData["FR6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR6\""));
                        ViewData["RL6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL6\""));
                        break;
                    case 6:
                        ViewBag.Year7 = "+7Year";

                        if (ViewData["IB7"] != null)
                        {
                            if (ViewData["IB7"].ToString() == "EoS") ViewData["IB7"] = string.Empty;
                        }
                        ViewData["IB7ForeColor"] = "black40";
                        ViewData["IB7TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR7TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS7TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL7TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB7\""));
                        ViewData["RS7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS7\""));
                        ViewData["FR7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR7\""));
                        ViewData["RL7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL7\""));
                        break;
                    case 7:
                        ViewBag.Year8 = "+8Year";

                        if (ViewData["IB8"] != null)
                        {
                            if (ViewData["IB8"].ToString() == "EoS") ViewData["IB8"] = string.Empty;
                        }
                        ViewData["IB8ForeColor"] = "black40";
                        ViewData["IB8TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR8TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS8TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL8TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB8\""));
                        ViewData["RS8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS8\""));
                        ViewData["FR8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR8\""));
                        ViewData["RL8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL8\""));
                        break;
                    case 8:
                        ViewBag.Year9 = "+9Year";

                        if (ViewData["IB9"] != null)
                        {
                            if (ViewData["IB9"].ToString() == "EoS") ViewData["IB9"] = string.Empty;
                        }
                        ViewData["IB9ForeColor"] = "black40";
                        ViewData["IB9TabIndex"] = _tabidx;
                        _tabidx += 1;
                        ViewData["FR9TabIndex"] = _tabidx + nbrOfServiceYears;
                        ViewData["RS9TabIndex"] = _tabidx + 2*nbrOfServiceYears;
                        ViewData["RL9TabIndex"] = _tabidx + 3*nbrOfServiceYears;
                        ViewData["IB9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"IB9\""));
                        ViewData["RS9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RS9\""));
                        ViewData["FR9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"FR9\""));
                        ViewData["RL9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("placeholder={0}", "\"RL9\""));
                        break;
                    case 9:
                        ViewData["IB10Disabled"] = string.Empty;
                        break;
                }
                yearCnt += 1;
            }

            _tabidx += 1 + 4*nbrOfServiceYears;
            switch (nbrOfServiceYears)
            {
                case 0:
                    ViewData["IB1"] = "EoS";

                    ViewData["IB1ForeColor"] = "red40";
                    ViewData["IB2ForeColor"] = "red40";
                    ViewData["IB3ForeColor"] = "red40";
                    ViewData["IB4ForeColor"] = "red40";
                    ViewData["IB5ForeColor"] = "red40";
                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB1TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 1:
                    ViewData["IB2"] = "EoS";

                    ViewData["IB2ForeColor"] = "red40";
                    ViewData["IB3ForeColor"] = "red40";
                    ViewData["IB4ForeColor"] = "red40";
                    ViewData["IB5ForeColor"] = "red40";
                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB2TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 2:
                    ViewData["IB3"] = "EoS";

                    ViewData["IB3ForeColor"] = "red40";
                    ViewData["IB4ForeColor"] = "red40";
                    ViewData["IB5ForeColor"] = "red40";
                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB3TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 3:
                    ViewData["IB4"] = "EoS";

                    ViewData["IB4ForeColor"] = "red40";
                    ViewData["IB5ForeColor"] = "red40";
                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB4TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 4:
                    ViewData["IB5"] = "EoS";

                    ViewData["IB5ForeColor"] = "red40";
                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB5TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 5:
                    ViewData["IB6"] = "EoS";

                    ViewData["IB6ForeColor"] = "red40";
                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB6TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 6:
                    ViewData["IB7"] = "EoS";

                    ViewData["IB7ForeColor"] = "red40";
                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB7TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 7:
                    ViewData["IB8"] = "EoS";

                    ViewData["IB8ForeColor"] = "red40";
                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB8TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 8:
                    ViewData["IB9"] = "EoS";

                    ViewData["IB9ForeColor"] = "red40";
                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB9TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 9:
                    ViewData["IB10"] = "EoS";

                    ViewData["IB10ForeColor"] = "red40";
                    ViewData["IB10TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
                case 10:
                    ViewData["IB10TabIndex"] = _tabidx + 3*nbrOfServiceYears + 1;
                    break;
            }
            _tabidx = 0;
            while (yearCnt <= LtbCommon.MaxYear)
            {
                switch (yearCnt)
                {
                    case 1:
                        ViewBag.Year1 = string.Empty;

                        if (nbrOfServiceYears != 0)
                        {
                            ViewData["IB1"] = string.Empty;
                            ViewData["IB1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS1"] = string.Empty;
                        ViewData["FR1"] = string.Empty;
                        ViewData["RL1"] = string.Empty;
                        ViewData["RS1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB1TabIndex"] = 0;
                        ViewData["RS1TabIndex"] = 0;
                        ViewData["FR1TabIndex"] = 0;
                        ViewData["RL1TabIndex"] = 0;

                        break;
                    case 2:
                        ViewBag.Year2 = string.Empty;

                        if (nbrOfServiceYears != 1)
                        {
                            ViewData["IB2"] = string.Empty;
                            ViewData["IB2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS2"] = string.Empty;
                        ViewData["FR2"] = string.Empty;
                        ViewData["RL2"] = string.Empty;
                        ViewData["RS2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB2TabIndex"] = 0;
                        ViewData["RS2TabIndex"] = 0;
                        ViewData["FR2TabIndex"] = 0;
                        ViewData["RL2TabIndex"] = 0;
                        break;
                    case 3:
                        ViewBag.Year3 = string.Empty;

                        if (nbrOfServiceYears != 2)
                        {
                            ViewData["IB3"] = string.Empty;
                            ViewData["IB3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS3"] = string.Empty;
                        ViewData["FR3"] = string.Empty;
                        ViewData["RL3"] = string.Empty;
                        ViewData["RS3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB3TabIndex"] = 0;
                        ViewData["RS3TabIndex"] = 0;
                        ViewData["FR3TabIndex"] = 0;
                        ViewData["RL3TabIndex"] = 0;
                        break;
                    case 4:
                        ViewBag.Year4 = string.Empty;

                        if (nbrOfServiceYears != 3)
                        {
                            ViewData["IB4"] = string.Empty;
                            ViewData["IB4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS4"] = string.Empty;
                        ViewData["FR4"] = string.Empty;
                        ViewData["RL4"] = string.Empty;
                        ViewData["RS4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB4TabIndex"] = 0;
                        ViewData["RS4TabIndex"] = 0;
                        ViewData["FR4TabIndex"] = 0;
                        ViewData["RL4TabIndex"] = 0;
                        break;
                    case 5:
                        ViewBag.Year5 = string.Empty;

                        if (nbrOfServiceYears != 4)
                        {
                            ViewData["IB5"] = string.Empty;
                            ViewData["IB5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS5"] = string.Empty;
                        ViewData["FR5"] = string.Empty;
                        ViewData["RL5"] = string.Empty;
                        ViewData["RS5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB5TabIndex"] = 0;
                        ViewData["RS5TabIndex"] = 0;
                        ViewData["FR5TabIndex"] = 0;
                        ViewData["RL5TabIndex"] = 0;
                        break;
                    case 6:
                        ViewBag.Year6 = string.Empty;

                        if (nbrOfServiceYears != 5)
                        {
                            ViewData["IB6"] = string.Empty;
                            ViewData["IB6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS6"] = string.Empty;
                        ViewData["FR6"] = string.Empty;
                        ViewData["RL6"] = string.Empty;
                        ViewData["RS6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB6TabIndex"] = 0;
                        ViewData["RS6TabIndex"] = 0;
                        ViewData["FR6TabIndex"] = 0;
                        ViewData["RL6TabIndex"] = 0;
                        break;
                    case 7:
                        ViewBag.Year7 = string.Empty;

                        if (nbrOfServiceYears != 6)
                        {
                            ViewData["IB7"] = string.Empty;
                            ViewData["IB7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS7"] = string.Empty;
                        ViewData["FR7"] = string.Empty;
                        ViewData["RL7"] = string.Empty;
                        ViewData["RS7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB7TabIndex"] = 0;
                        ViewData["RS7TabIndex"] = 0;
                        ViewData["FR7TabIndex"] = 0;
                        ViewData["RL7TabIndex"] = 0;
                        break;
                    case 8:
                        ViewBag.Year8 = string.Empty;

                        if (nbrOfServiceYears != 7)
                        {
                            ViewData["IB8"] = string.Empty;
                            ViewData["IB8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS8"] = string.Empty;
                        ViewData["FR8"] = string.Empty;
                        ViewData["RL8"] = string.Empty;
                        ViewData["RS8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB8TabIndex"] = 0;
                        ViewData["RS8TabIndex"] = 0;
                        ViewData["FR8TabIndex"] = 0;
                        ViewData["RL8TabIndex"] = 0;
                        break;
                    case 9:
                        ViewBag.Year9 = string.Empty;

                        if (nbrOfServiceYears != 8)
                        {
                            ViewData["IB9"] = string.Empty;
                            ViewData["IB9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["RS9"] = string.Empty;
                        ViewData["FR9"] = string.Empty;
                        ViewData["RL9"] = string.Empty;
                        ViewData["RS9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["FR9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["RL9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        ViewData["IB9TabIndex"] = 0;
                        ViewData["RS9TabIndex"] = 0;
                        ViewData["FR9TabIndex"] = 0;
                        ViewData["RL9TabIndex"] = 0;
                        break;
                    case 10:
                        ViewBag.Year10 = string.Empty;

                        if (nbrOfServiceYears != 9)
                        {
                            ViewData["IB10"] = string.Empty;
                            ViewData["IB10Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                        }
                        ViewData["IB10TabIndex"] = 0;
                        break;
                }
                yearCnt += 1;
                AdjustRepair(repairAvailable);
            }
        }

        private void AdjustRepair(bool repairAvailable)
        {
            var cnt = 0;
            var nbrOfServiceYears = 0;
            cnt = 0;
            var ci = new CultureInfo("sv-SE");
            LifeTimeBuy = Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci);
            EndOfService = Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci);
            nbrOfServiceYears = ServiceYears;
            while (cnt <= nbrOfServiceYears)
            {
                switch (cnt)
                {
                    case 0:
                        if (!repairAvailable)
                        {
                            ViewData["RL0Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL0"] = "100";
                        }
                        else
                        {
                            ViewData["RL0Disabled"] = string.Empty;
                        }
                        break;
                    case 1:
                        if (!repairAvailable)
                        {
                            ViewData["RL1Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL1"] = "100";
                        }
                        else
                        {
                            ViewData["RL1Disabled"] = string.Empty;
                        }
                        break;
                    case 2:
                        if (!repairAvailable)
                        {
                            ViewData["RL2Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL2"] = "100";
                        }
                        else
                        {
                            ViewData["RL2Disabled"] = string.Empty;
                        }
                        break;
                    case 3:
                        if (!repairAvailable)
                        {
                            ViewData["RL3Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL3"] = "100";
                        }
                        else
                        {
                            ViewData["RL3Disabled"] = string.Empty;
                        }
                        break;
                    case 4:
                        if (!repairAvailable)
                        {
                            ViewData["RL4Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL4"] = "100";
                        }
                        else
                        {
                            ViewData["RL4Disabled"] = string.Empty;
                        }
                        break;
                    case 5:
                        if (!repairAvailable)
                        {
                            ViewData["RL5Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL5"] = "100";
                        }
                        else
                        {
                            ViewData["RL5Disabled"] = string.Empty;
                        }
                        break;
                    case 6:
                        if (!repairAvailable)
                        {
                            ViewData["RL6Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL6"] = "100";
                        }
                        else
                        {
                            ViewData["RL6Disabled"] = string.Empty;
                        }
                        break;
                    case 7:
                        if (!repairAvailable)
                        {
                            ViewData["RL7Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL7"] = "100";
                        }
                        else
                        {
                            ViewData["RL7Disabled"] = string.Empty;
                        }
                        break;
                    case 8:
                        if (!repairAvailable)
                        {
                            ViewData["RL8Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL8"] = "100";
                        }
                        else
                        {
                            ViewData["RL8Disabled"] = string.Empty;
                        }
                        break;
                    case 9:
                        if (!repairAvailable)
                        {
                            ViewData["RL9Disabled"] = new System.Web.Mvc.MvcHtmlString(string.Format("disabled={0}", "\"disabled\""));
                            ViewData["RL9"] = "100";
                        }
                        else
                        {
                            ViewData["RL9Disabled"] = string.Empty;
                        }

                        break;
                }
                cnt += 1;
            }
        }

        private void GetInputData(int finalYear, ref double confidenceLevelFromNormsInv)
        {
            var ci = new CultureInfo("sv-SE");
            var startDate = Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci);
            var endOfService = Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci);
            LifeTimeBuy = startDate;
            EndOfService = endOfService;
            var confidenceSelection = Convert.ToInt32(ViewData["ConfLevelInPercent"].ToString());
            switch (confidenceSelection)
            {
                //Confidence Level

                case 60:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.6);
                    _confidenceLevelConverted = 0.6;

                    break;
                case 70:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.7);
                    _confidenceLevelConverted = 0.7;

                    break;
                case 80:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.8);
                    _confidenceLevelConverted = 0.8;

                    break;
                case 90:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.9);
                    _confidenceLevelConverted = 0.9;

                    break;
                case 95:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.95);
                    _confidenceLevelConverted = 0.95;

                    break;
                case 995:
                    confidenceLevelFromNormsInv = LtbCommon.GetConfidenceLevelFromNormsInv(0.995);
                    _confidenceLevelConverted = 0.995;

                    break;
            }

            var yearCnt = 0;

            while (yearCnt <= finalYear)
            {
                switch (yearCnt)
                {
                    case 0:
                        ViewData["RS0ForeColor"] = Color.Black;
                        InstalledBasePerYear[0] = Convert.ToInt32(ViewData["IB0"].ToString());
                        RegionalStocksPerYear[0] = Convert.ToInt32(ViewData["RS0"].ToString());
                        FailureRatePerYear[0] = Convert.ToDouble(ViewData["FR0"].ToString(), ci);
                        RepairLossPerYear[0] = Convert.ToDouble(Convert.ToDouble(ViewData["RL0"].ToString())/100, ci);

                        break;
                    case 1:
                        ViewData["RS1ForeColor"] = Color.Black;
                        InstalledBasePerYear[1] = Convert.ToInt32(ViewData["IB1"].ToString());
                        RegionalStocksPerYear[1] = Convert.ToInt32(ViewData["RS1"].ToString());
                        FailureRatePerYear[1] = Convert.ToDouble(ViewData["FR1"].ToString(), ci);
                        RepairLossPerYear[1] = Convert.ToDouble(Convert.ToDouble(ViewData["RL1"].ToString())/100, ci);

                        break;
                    case 2:
                        ViewData["RS2ForeColor"] = Color.Black;
                        InstalledBasePerYear[2] = Convert.ToInt32(ViewData["IB2"].ToString());
                        RegionalStocksPerYear[2] = Convert.ToInt32(ViewData["RS2"].ToString());
                        FailureRatePerYear[2] = Convert.ToDouble(ViewData["FR2"].ToString(), ci);
                        RepairLossPerYear[2] = Convert.ToDouble(Convert.ToDouble(ViewData["RL2"].ToString())/100, ci);

                        break;
                    case 3:
                        ViewData["RS3ForeColor"] = Color.Black;
                        InstalledBasePerYear[3] = Convert.ToInt32(ViewData["IB3"].ToString());
                        RegionalStocksPerYear[3] = Convert.ToInt32(ViewData["RS3"].ToString());
                        FailureRatePerYear[3] = Convert.ToDouble(ViewData["FR3"].ToString(), ci);
                        RepairLossPerYear[3] = Convert.ToDouble(Convert.ToDouble(ViewData["RL3"].ToString())/100, ci);

                        break;
                    case 4:
                        ViewData["RS4ForeColor"] = Color.Black;
                        InstalledBasePerYear[4] = Convert.ToInt32(ViewData["IB4"].ToString());
                        RegionalStocksPerYear[4] = Convert.ToInt32(ViewData["RS4"].ToString());
                        FailureRatePerYear[4] = Convert.ToDouble(ViewData["FR4"].ToString(), ci);
                        RepairLossPerYear[4] = Convert.ToDouble(Convert.ToDouble(ViewData["RL4"].ToString())/100, ci);

                        break;
                    case 5:
                        ViewData["RS5ForeColor"] = Color.Black;
                        InstalledBasePerYear[5] = Convert.ToInt32(ViewData["IB5"].ToString());
                        RegionalStocksPerYear[5] = Convert.ToInt32(ViewData["RS5"].ToString());
                        FailureRatePerYear[5] = Convert.ToDouble(ViewData["FR5"].ToString(), ci);
                        RepairLossPerYear[5] = Convert.ToDouble(Convert.ToDouble(ViewData["RL5"].ToString())/100, ci);

                        break;
                    case 6:
                        ViewData["RS6ForeColor"] = Color.Black;
                        InstalledBasePerYear[6] = Convert.ToInt32(ViewData["IB6"].ToString());
                        RegionalStocksPerYear[6] = Convert.ToInt32(ViewData["RS6"].ToString());
                        FailureRatePerYear[6] = Convert.ToDouble(ViewData["FR6"].ToString(), ci);
                        RepairLossPerYear[6] = Convert.ToDouble(Convert.ToDouble(ViewData["RL6"].ToString())/100, ci);

                        break;
                    case 7:
                        ViewData["RS7ForeColor"] = Color.Black;
                        InstalledBasePerYear[7] = Convert.ToInt32(ViewData["IB7"].ToString());
                        RegionalStocksPerYear[7] = Convert.ToInt32(ViewData["RS7"].ToString());
                        FailureRatePerYear[7] = Convert.ToDouble(ViewData["FR7"].ToString(), ci);
                        RepairLossPerYear[7] = Convert.ToDouble(Convert.ToDouble(ViewData["RL7"].ToString())/100, ci);

                        break;
                    case 8:
                        ViewData["RS8ForeColor"] = Color.Black;
                        InstalledBasePerYear[8] = Convert.ToInt32(ViewData["IB8"].ToString());
                        RegionalStocksPerYear[8] = Convert.ToInt32(ViewData["RS8"].ToString());
                        FailureRatePerYear[8] = Convert.ToDouble(ViewData["FR8"].ToString(), ci);
                        RepairLossPerYear[8] = Convert.ToDouble(Convert.ToDouble(ViewData["RL8"].ToString())/100, ci);

                        break;
                    case 9:
                        ViewData["RS9ForeColor"] = Color.Black;
                        InstalledBasePerYear[9] = Convert.ToInt32(ViewData["IB9"].ToString());
                        RegionalStocksPerYear[9] = Convert.ToInt32(ViewData["RS9"].ToString());
                        FailureRatePerYear[9] = Convert.ToDouble(ViewData["FR9"].ToString(), ci);
                        RepairLossPerYear[9] = Convert.ToDouble(Convert.ToDouble(ViewData["RL9"].ToString())/100, ci);

                        break;
                }
                yearCnt++;
            }

            AdjustForecolorAndClearRemains(yearCnt);
        }

        private void AdjustForecolorAndClearRemains(int startYear)
        {
            var yearCnt = startYear;
            while (yearCnt <= LtbCommon.MaxYear)
            {
                switch (yearCnt)
                {
                    case 0:
                        ViewData["RS0ForeColor"] = Color.Black;
                        ViewData["IB0"] = string.Empty;
                        ViewData["RS0"] = string.Empty;
                        ViewData["FR0"] = string.Empty;
                        ViewData["RL0"] = string.Empty;
                        ViewBag.Year0 = string.Empty;
                        ViewData["RS1ForeColor"] = Color.Black;
                        ViewData["IB1"] = string.Empty;
                        ViewData["RS1"] = string.Empty;
                        ViewData["FR1"] = string.Empty;
                        ViewData["RL1"] = string.Empty;
                        ViewBag.Year1 = string.Empty;

                        break;
                    case 1:
                        ViewData["RS2ForeColor"] = Color.Black;
                        ViewData["IB2"] = string.Empty;
                        ViewData["RS2"] = string.Empty;
                        ViewData["FR2"] = string.Empty;
                        ViewData["RL2"] = string.Empty;
                        ViewData["IB2TabIndex"] = 0;
                        ViewData["RS2TabIndex"] = 0;
                        ViewData["FR2TabIndex"] = 0;
                        ViewData["RL2TabIndex"] = 0;
                        ViewBag.Year2 = string.Empty;
                        break;
                    case 2:
                        ViewData["RS3ForeColor"] = Color.Black;
                        ViewData["IB3"] = string.Empty;
                        ViewData["RS3"] = string.Empty;
                        ViewData["FR3"] = string.Empty;
                        ViewData["RL3"] = string.Empty;
                        ViewData["IB3TabIndex"] = 0;
                        ViewData["RS3TabIndex"] = 0;
                        ViewData["FR3TabIndex"] = 0;
                        ViewData["RL3TabIndex"] = 0;
                        ViewBag.Year3 = string.Empty;
                        break;
                    case 3:
                        ViewData["RS4ForeColor"] = Color.Black;
                        ViewData["IB4"] = string.Empty;
                        ViewData["RS4"] = string.Empty;
                        ViewData["FR4"] = string.Empty;
                        ViewData["RL4"] = string.Empty;
                        ViewData["IB4TabIndex"] = 0;
                        ViewData["RS4TabIndex"] = 0;
                        ViewData["FR4TabIndex"] = 0;
                        ViewData["RL4TabIndex"] = 0;
                        ViewBag.Year4 = string.Empty;
                        break;
                    case 4:
                        ViewData["RS5ForeColor"] = Color.Black;
                        ViewData["IB5"] = string.Empty;
                        ViewData["RS5"] = string.Empty;
                        ViewData["FR5"] = string.Empty;
                        ViewData["RL5"] = string.Empty;
                        ViewData["IB5TabIndex"] = 0;
                        ViewData["RS5TabIndex"] = 0;
                        ViewData["FR5TabIndex"] = 0;
                        ViewData["RL5TabIndex"] = 0;
                        ViewBag.Year5 = string.Empty;
                        break;
                    case 5:
                        ViewData["RS6ForeColor"] = Color.Black;
                        ViewData["IB6"] = string.Empty;
                        ViewData["RS6"] = string.Empty;
                        ViewData["FR6"] = string.Empty;
                        ViewData["RL6"] = string.Empty;
                        ViewData["IB6TabIndex"] = 0;
                        ViewData["FS62TabIndex"] = 0;
                        ViewData["FR6TabIndex"] = 0;
                        ViewData["RL6TabIndex"] = 0;
                        ViewBag.Year6 = string.Empty;
                        break;
                    case 6:
                        ViewData["RS7ForeColor"] = Color.Black;
                        ViewData["IB7"] = string.Empty;
                        ViewData["RS7"] = string.Empty;
                        ViewData["FR7"] = string.Empty;
                        ViewData["RL7"] = string.Empty;
                        ViewData["IB7TabIndex"] = 0;
                        ViewData["RS7TabIndex"] = 0;
                        ViewData["FR7TabIndex"] = 0;
                        ViewData["RL7TabIndex"] = 0;
                        ViewBag.Year7 = string.Empty;
                        break;
                    case 7:
                        ViewData["RS8ForeColor"] = Color.Black;
                        ViewData["IB8"] = string.Empty;
                        ViewData["RS8"] = string.Empty;
                        ViewData["FR8"] = string.Empty;
                        ViewData["RL8"] = string.Empty;
                        ViewData["IB8TabIndex"] = 0;
                        ViewData["RS8TabIndex"] = 0;
                        ViewData["FR8TabIndex"] = 0;
                        ViewData["RL8TabIndex"] = 0;
                        ViewBag.Year8 = string.Empty;
                        break;
                    case 8:
                        ViewData["RS9ForeColor"] = Color.Black;
                        ViewData["IB9"] = string.Empty;
                        ViewData["RS9"] = string.Empty;
                        ViewData["FR9"] = string.Empty;
                        ViewData["RL9"] = string.Empty;
                        ViewData["IB9TabIndex"] = 0;
                        ViewData["RS9TabIndex"] = 0;
                        ViewData["FR9TabIndex"] = 0;
                        ViewData["RL9TabIndex"] = 0;
                        ViewBag.Year9 = string.Empty;
                        break;

                    case 9:
                        ViewData["IB10TabIndex"] = 0;
                        break;
                    case 10:
                        break;
                }
                yearCnt += 1;
            }
        }

        private bool BoundariesOk(int finalYear)
        {
            var functionReturnValue = false;

            var ci = new CultureInfo("sv-SE");
            var tmpDate = Convert.ToDateTime(ViewData["LTBDate"], ci);
            var yearCnt = 0;
            functionReturnValue = true;
            while (yearCnt <= finalYear)
            {
                var tmp = "";
                switch (yearCnt)
                {
                    case 0:
                        tmp = ViewData["FR0"].ToString();

                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR0"] = tmp;
                        if (ViewData["RS0"].ToString() == string.Empty)
                            ViewData["RS0"] = "0";
                        if (!Information.IsNumeric(ViewData["IB0"].ToString()) |
                            !Information.IsNumeric(ViewData["RS0"].ToString()) |
                            !Information.IsNumeric(ViewData["FR0"].ToString()) |
                            !Information.IsNumeric(ViewData["RL0"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = string.Format("Fel parameter för" + "{0}", tmpDate.Year);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB0"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB0"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS0"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS0"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR0"].ToString(), ci) < 1E-07 |
                            Convert.ToDouble(ViewData["FR0"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL0"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL0"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + tmpDate.Year;
                            return functionReturnValue;
                        }
                        break;
                    case 1:
                        tmp = ViewData["FR1"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR1"] = tmp;
                        if (ViewData["RS1"].ToString() == string.Empty)
                            ViewData["RS1"] = "0";
                        if (!Information.IsNumeric(ViewData["IB1"].ToString()) |
                            !Information.IsNumeric(ViewData["RS1"].ToString()) |
                            !Information.IsNumeric(ViewData["FR1"].ToString()) |
                            !Information.IsNumeric(ViewData["RL1"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 1);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB1"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB1"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS1"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS1"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR1"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR1"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL1"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL1"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 1);
                            return functionReturnValue;
                        }
                        break;
                    case 2:
                        tmp = ViewData["FR2"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR2"] = tmp;
                        if (ViewData["RS2"].ToString() == string.Empty)
                            ViewData["RS2"] = "0";
                        if (!Information.IsNumeric(ViewData["IB2"].ToString()) |
                            !Information.IsNumeric(ViewData["RS2"].ToString()) |
                            !Information.IsNumeric(ViewData["FR2"].ToString()) |
                            !Information.IsNumeric(ViewData["RL2"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 2);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB2"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB2"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS2"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS2"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR2"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR2"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL2"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL2"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 2);
                            return functionReturnValue;
                        }
                        break;
                    case 3:
                        tmp = ViewData["FR3"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR3"] = tmp;
                        if (ViewData["RS3"].ToString() == string.Empty)
                            ViewData["RS3"] = "0";
                        if (!Information.IsNumeric(ViewData["IB3"].ToString()) |
                            !Information.IsNumeric(ViewData["RS3"].ToString()) |
                            !Information.IsNumeric(ViewData["FR3"].ToString()) |
                            !Information.IsNumeric(ViewData["RL3"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 3);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB3"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB3"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS3"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS3"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR3"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR3"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL3"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL3"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 3);
                            return functionReturnValue;
                        }
                        break;
                    case 4:
                        tmp = ViewData["FR4"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR4"] = tmp;
                        if (ViewData["RS4"].ToString() == string.Empty)
                            ViewData["RS4"] = "0";
                        if (!Information.IsNumeric(ViewData["IB4"].ToString()) |
                            !Information.IsNumeric(ViewData["RS4"].ToString()) |
                            !Information.IsNumeric(ViewData["FR4"].ToString()) |
                            !Information.IsNumeric(ViewData["RL4"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 4);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB4"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB4"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS4"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS4"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR4"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR4"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL4"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL4"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 4);
                            return functionReturnValue;
                        }
                        break;
                    case 5:
                        tmp = ViewData["FR5"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR5"] = tmp;
                        if (ViewData["RS5"].ToString() == string.Empty)
                            ViewData["RS5"] = "0";
                        if (!Information.IsNumeric(ViewData["IB5"].ToString()) |
                            !Information.IsNumeric(ViewData["RS5"].ToString()) |
                            !Information.IsNumeric(ViewData["FR5"].ToString()) |
                            !Information.IsNumeric(ViewData["RL5"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 5);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB5"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB5"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS5"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS5"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR5"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR5"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL5"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL5"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 5);
                            return functionReturnValue;
                        }
                        break;
                    case 6:
                        tmp = ViewData["FR6"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR6"] = tmp;
                        if (ViewData["RS6"].ToString() == string.Empty)
                            ViewData["RS6"] = "0";
                        if (!Information.IsNumeric(ViewData["IB6"].ToString()) |
                            !Information.IsNumeric(ViewData["RS6"].ToString()) |
                            !Information.IsNumeric(ViewData["FR6"].ToString()) |
                            !Information.IsNumeric(ViewData["RL6"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 6);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB6"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB6"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS6"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS6"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR6"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR6"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL6"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL6"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 6);
                            return functionReturnValue;
                        }
                        break;
                    case 7:
                        tmp = ViewData["FR7"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR7"] = tmp;
                        if (ViewData["RS7"].ToString() == string.Empty)
                            ViewData["RS7"] = "0";
                        if (!Information.IsNumeric(ViewData["IB7"].ToString()) |
                            !Information.IsNumeric(ViewData["RS7"].ToString()) |
                            !Information.IsNumeric(ViewData["FR7"].ToString()) |
                            !Information.IsNumeric(ViewData["RL7"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 7);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB7"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB7"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS7"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS7"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR7"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR7"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL7"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL7"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 7);
                            return functionReturnValue;
                        }
                        break;
                    case 8:
                        tmp = ViewData["FR8"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR8"] = tmp;
                        if (ViewData["RS8"].ToString() == string.Empty)
                            ViewData["RS8"] = "0";
                        if (!Information.IsNumeric(ViewData["IB8"].ToString()) |
                            !Information.IsNumeric(ViewData["RS8"].ToString()) |
                            !Information.IsNumeric(ViewData["FR8"].ToString()) |
                            !Information.IsNumeric(ViewData["RL8"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 8);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB8"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB8"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS8"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS8"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR8"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR8"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL8"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL8"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 8);
                            return functionReturnValue;
                        }
                        break;
                    case 9:
                        tmp = ViewData["FR9"].ToString();
                        if (tmp != "") tmp = Strings.Replace(tmp, ".", ",");
                        ViewData["FR9"] = tmp;
                        if (ViewData["RS9"].ToString() == string.Empty)
                            ViewData["RS9"] = "0";
                        if (!Information.IsNumeric(ViewData["IB9"].ToString()) |
                            !Information.IsNumeric(ViewData["RS9"].ToString()) |
                            !Information.IsNumeric(ViewData["FR9"].ToString()) |
                            !Information.IsNumeric(ViewData["RL9"].ToString()))
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 9);
                            return functionReturnValue;
                        }
                        if (Convert.ToInt32(ViewData["IB9"].ToString()) > 999999 |
                            Convert.ToInt32(ViewData["IB9"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS9"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RS9"].ToString()) > 9999 |
                            Convert.ToDouble(ViewData["FR9"].ToString(), ci) < 1E-06 |
                            Convert.ToDouble(ViewData["FR9"].ToString(), ci) > 100 |
                            Convert.ToInt32(ViewData["RL9"].ToString()) < 0 |
                            Convert.ToInt32(ViewData["RL9"].ToString()) > 100)
                        {
                            functionReturnValue = false;
                            yearCnt = finalYear;
                            _textInfo = "Fel parameter för" + (Convert.ToInt32(tmpDate.Year) + 9);
                            return functionReturnValue;
                        }
                        break;
                    case 10:
                        break;
                }
                yearCnt += 1;
            }
            return functionReturnValue;
        }

        public static int RoundUpInt(double x, int y)
        {
            return Convert.ToInt32(Math.Round(x + 0.49999999999, y));
        }

        private int Calculate_Click()
        {
            if (!Information.IsNumeric(ViewData["RepairLeadDays"].ToString()))
            {
                _textInfo = "Repair Lead Time kan inte vara tom!";
                ViewData["InfoText"] = _textInfo;
                return -1;
            }
            _leadDays = Convert.ToInt32(ViewData["RepairLeadDays"].ToString());
            if (_leadDays < LtbCommon.MinLeadDays | _leadDays > LtbCommon.MaxLeadDays)
            {
                _textInfo = "Fel: 2 <= Repair Lead Time <=365";
                ViewData["InfoText"] = _textInfo;
                return -1;
            }

            if (_leadDays > ServiceDays)
            {
                ClearResult();
                _textInfo =
                    "Fel: Repair Lead Time kan inte vara längre än serviceperioden. Vänligen ändra EoS eller Repair Lead Time";
                ViewData["InfoText"] = _textInfo;
                return -1;
            }

            if (ServiceDays > LtbCommon.MaxServiceDays)
            {
                ClearResult();
                _textInfo = "Fel: Serviceperioden får ej vara längre än 10 år. Vänligen ändra EoS eller LTB";
                ViewData["InfoText"] = _textInfo;
                return -1;
            }

            _nbrOfSamples = RoundUpInt(ServiceDays/(double) _leadDays, 0);

            if (!BoundariesOk(ServiceYears))
            {
                ViewData["InfoText"] = _textInfo;
                return -1;
            }


            GetInputData(ServiceYears, ref _confidenceLevelFromNormsInv);
            _ltbChart = new MemoryStream();
            var ltb = new LtbCommon();
            ltb.LtbWorker(_nbrOfSamples,
                ServiceDays,
                ServiceYears,
                _leadDays,
                out _stock,
                out _safety,
                out _failed,
                out _repaired,
                out _lost,
                out _total,
                _confidenceLevelFromNormsInv,
                _confidenceLevelConverted,
                InstalledBasePerYear,
                FailureRatePerYear,
                RepairLossPerYear,
                RegionalStocksPerYear,
                out _ltbChart,
                Startup.RootPathUpload);

            ViewData["Stock"] = _stock;
            ViewData["Safety"] = _safety;
            ViewData["Total"] = _total;
            ViewData["ServiceDays"] = ServiceDays;
            ViewData["Failed"] = _failed;
            ViewData["Repaired"] = _repaired;
            ViewData["Lost"] = _lost;
            return 0;
        }

        private void ClearResult()
        {
            _ltbChart = null;
            ViewData["Stock"] = string.Empty;
            ViewData["Safety"] = string.Empty;
            ViewData["InfoText"] = string.Empty;
            ViewData["Failed"] = string.Empty;
            ViewData["Repaired"] = string.Empty;
            ViewData["Lost"] = string.Empty;
            ViewData["Total"] = string.Empty;
            ViewData["ServiceDays"] = string.Empty;
        }

        private void InitConfidenceDropDown_CheckUI()
        {
            CheckErrorAlert();
            var dropDowns = new[]
            {
                new DropDown {Value = "60", Text = "60%"}, new DropDown {Value = "70", Text = "70%"},
                new DropDown {Value = "80", Text = "80%"}, new DropDown {Value = "90", Text = "90%"},
                new DropDown {Value = "95", Text = "95%"}, new DropDown {Value = "995", Text = "99,5%"}
            };
            var selectList = new SelectList(dropDowns, "Value", "Text", ViewData["ConfLevelInPercent"].ToString());

            ViewData["ConfidenceLevels"] = selectList;
            ViewData["About"] = "";
        }


        private void Calculateen(string confidenceLevelInPercent, string RLD, string LTBMM, string LTBDD, string LTBYY,
            string EOSMM, string EOSDD, string EOSYY, string IB0, string IB1, string IB2, string IB3, string IB4,
            string IB5, string IB6, string IB7, string IB8, string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTBMM + '/' + LTBDD + '/' + LTBYY,
                EOSMM + '/' + EOSDD + '/' + EOSYY, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8, IB9, IB10, RS0, RS1, RS2,
                RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();
            _repairSelected = GetRepairSelected();
            InitYearTabIndex(_repairSelected);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (Calculate_Click() == -1)
            {
                //ViewData["ErrorAlert"] = "ErrorAlert";
            }
        }

        private void Calculatesv(string confidenceLevelInPercent, string RLD, string LTB, string EOS, string IB0,
            string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8, string IB9,
            string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTB, EOS, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8,
                IB9, IB10, RS0, RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();
            _repairSelected = GetRepairSelected();
            InitYearTabIndex(_repairSelected);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (Calculate_Click() == -1)
            {
                //ViewData["ErrorAlert"] = "ErrorAlert";
            }
            var ci = new CultureInfo("sv-SE");
            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
        }


        private void CopyInputToViewData(string confidenceLeveleInPercent, string RLD, string LTB, string EOS,
            string IB0, string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8,
            string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            var ci = new CultureInfo("sv-SE");
            LifeTimeBuy = Convert.ToDateTime(LTB, ci);
            EndOfService = Convert.ToDateTime(EOS, ci);
            ViewData["ConfLevelInPercent"] = confidenceLeveleInPercent;
            ViewData["RepairLeadDays"] = Convert.ToInt32(RLD);

            ViewData["LTBDate"] = LTB;
            ViewData["EOSDate"] = EOS;
            ViewData["InfoText"] = string.Empty;
            ViewData["IB0"] = IB0.Substring(1, IB0.Length - 1);
            ViewData["IB1"] = IB1.Substring(1, IB1.Length - 1);
            ViewData["IB2"] = IB2.Substring(1, IB2.Length - 1);
            ViewData["IB3"] = IB3.Substring(1, IB3.Length - 1);
            ViewData["IB4"] = IB4.Substring(1, IB4.Length - 1);
            ViewData["IB5"] = IB5.Substring(1, IB5.Length - 1);
            ViewData["IB6"] = IB6.Substring(1, IB6.Length - 1);
            ViewData["IB7"] = IB7.Substring(1, IB7.Length - 1);
            ViewData["IB8"] = IB8.Substring(1, IB8.Length - 1);
            ViewData["IB9"] = IB9.Substring(1, IB9.Length - 1);
            ViewData["IB10"] = IB10.Substring(1, IB10.Length - 1);

            ViewData["RS0"] = RS0.Substring(1, RS0.Length - 1);
            ViewData["RS1"] = RS1.Substring(1, RS1.Length - 1);
            ViewData["RS2"] = RS2.Substring(1, RS2.Length - 1);
            ViewData["RS3"] = RS3.Substring(1, RS3.Length - 1);
            ViewData["RS4"] = RS4.Substring(1, RS4.Length - 1);
            ViewData["RS5"] = RS5.Substring(1, RS5.Length - 1);
            ViewData["RS6"] = RS6.Substring(1, RS6.Length - 1);
            ViewData["RS7"] = RS7.Substring(1, RS7.Length - 1);
            ViewData["RS8"] = RS8.Substring(1, RS8.Length - 1);
            ViewData["RS9"] = RS9.Substring(1, RS9.Length - 1);

            ViewData["FR0"] = FR0.Substring(1, FR0.Length - 1);
            ViewData["FR1"] = FR1.Substring(1, FR1.Length - 1);
            ViewData["FR2"] = FR2.Substring(1, FR2.Length - 1);
            ViewData["FR3"] = FR3.Substring(1, FR3.Length - 1);
            ViewData["FR4"] = FR4.Substring(1, FR4.Length - 1);
            ViewData["FR5"] = FR5.Substring(1, FR5.Length - 1);
            ViewData["FR6"] = FR6.Substring(1, FR6.Length - 1);
            ViewData["FR7"] = FR7.Substring(1, FR7.Length - 1);
            ViewData["FR8"] = FR8.Substring(1, FR8.Length - 1);
            ViewData["FR9"] = FR9.Substring(1, FR9.Length - 1);

            ViewData["RL0"] = RL0.Substring(1, RL0.Length - 1);
            ViewData["RL1"] = RL1.Substring(1, RL1.Length - 1);
            ViewData["RL2"] = RL2.Substring(1, RL2.Length - 1);
            ViewData["RL3"] = RL3.Substring(1, RL3.Length - 1);
            ViewData["RL4"] = RL4.Substring(1, RL4.Length - 1);
            ViewData["RL5"] = RL5.Substring(1, RL5.Length - 1);
            ViewData["RL6"] = RL6.Substring(1, RL6.Length - 1);
            ViewData["RL7"] = RL7.Substring(1, RL7.Length - 1);
            ViewData["RL8"] = RL8.Substring(1, RL8.Length - 1);
            ViewData["RL9"] = RL9.Substring(1, RL9.Length - 1);
            _repairSelected = GetRepairSelected();

            if (!_repairSelected)
            {
                ViewData["Repair"] = "javascript:SetRepair();";
                ViewData["NoRepair"] = "";
                ViewData["RepairChecked"] = "";
                ViewData["NoRepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            }
            else
            {
                ViewData["Repair"] = "";
                ViewData["NoRepair"] = "javascript:SetNoRepair();";
                ViewData["RepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
                ViewData["NoRepairChecked"] = "";
            }
        }

        private bool GetRepairSelected()
        {
            var nbrOfServiceYears = ServiceYears;
            if (nbrOfServiceYears > 9)
            {
                nbrOfServiceYears = 9;
            }
            var repairAvailable = true;
            switch (nbrOfServiceYears)
            {
                case 0:
                    if (ViewData["RL0"].ToString() == "100")
                    {
                        repairAvailable = false;
                    }
                    break;
                case 1:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 2:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 3:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 4:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 5:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100") && (ViewData["RL5"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 6:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100") && (ViewData["RL5"].ToString() == "100") &&
                        (ViewData["RL6"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 7:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100") && (ViewData["RL5"].ToString() == "100") &&
                        (ViewData["RL6"].ToString() == "100") && (ViewData["RL7"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 8:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100") && (ViewData["RL5"].ToString() == "100") &&
                        (ViewData["RL6"].ToString() == "100") && (ViewData["RL7"].ToString() == "100") &&
                        (ViewData["RL8"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
                case 9:
                    if ((ViewData["RL0"].ToString() == "100") && (ViewData["RL1"].ToString() == "100") &&
                        (ViewData["RL2"].ToString() == "100") && (ViewData["RL3"].ToString() == "100") &&
                        (ViewData["RL4"].ToString() == "100") && (ViewData["RL5"].ToString() == "100") &&
                        (ViewData["RL6"].ToString() == "100") && (ViewData["RL7"].ToString() == "100") &&
                        (ViewData["RL8"].ToString() == "100") && (ViewData["RL9"].ToString() == "100"))
                    {
                        repairAvailable = false;
                    }
                    break;
            }
            return repairAvailable;
        }

        private void SetDefault_LTB_EOSDate()
        {
            var today = DateTime.Now.Date;
            var eosDate = today.AddDays(LtbCommon.MaxServiceDays);

            LifeTimeBuy = today;
            EndOfService = eosDate;
            var ci = new CultureInfo("sv-SE");
            ViewData["LTBDate"] = today.ToString(ci);
            ViewData["LTBDate"] = ViewData["LTBDate"].ToString()
                .Substring(0, ViewData["LTBDate"].ToString().IndexOf(" "));

            ViewData["EOSDate"] = eosDate.ToString(ci);
            ViewData["EOSDate"] = ViewData["EOSDate"].ToString()
                .Substring(0, ViewData["EOSDate"].ToString().IndexOf(" "));
        }

        private void Repairsv(string confidenceLevelInPercent, string RLD, string LTB, string EOS, string IB0,
            string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8, string IB9,
            string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTB, EOS, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8,
                IB9, IB10, RS0, RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);

            ViewBag.SetFocus = "IB0";
            ViewData["Repair"] = "";
            ViewData["NoRepair"] = "javascript:SetNoRepair();";
            ViewData["RepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            ViewData["NoRepairChecked"] = "";
            ClearResult();
            InitYearTabIndex(true);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
        }

        private void Repairen(string confidenceLevelInPercent, string RLD, string LTBMM, string LTBDD, string LTBYY,
            string EOSMM, string EOSDD, string EOSYY, string IB0, string IB1, string IB2, string IB3, string IB4,
            string IB5, string IB6, string IB7, string IB8, string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTBMM + '/' + LTBDD + '/' + LTBYY,
                EOSMM + '/' + EOSDD + '/' + EOSYY, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8, IB9, IB10, RS0, RS1, RS2,
                RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);

            ViewData["Repair"] = "";
            ViewData["NoRepair"] = "javascript:SetNoRepair();";
            ViewData["RepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            ViewData["NoRepairChecked"] = "";
            ClearResult();
            InitYearTabIndex(true);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
        }

        private void NoRepairsv(string confidenceLevelInPercent, string RLD, string LTB, string EOS, string IB0,
            string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8, string IB9,
            string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTB, EOS, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8,
                IB9, IB10, RS0, RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);

            SetPageDefaultValues();
            ViewData["Repair"] = "javascript:SetRepair();";
            ViewData["NoRepair"] = "";
            ViewData["RepairChecked"] = "";
            ViewData["NoRepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            ClearResult();
            InitYearTabIndex(false);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
        }

        private void NoRepairen(string confidenceLevelInPercent, string RLD, string LTBMM, string LTBDD, string LTBYY,
            string EOSMM, string EOSDD, string EOSYY, string IB0, string IB1, string IB2, string IB3, string IB4,
            string IB5, string IB6, string IB7, string IB8, string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTBMM + '/' + LTBDD + '/' + LTBYY,
                EOSMM + '/' + EOSDD + '/' + EOSYY, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8, IB9, IB10, RS0, RS1, RS2,
                RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);

            SetPageDefaultValues();
            ViewData["Repair"] = "javascript:SetRepair();";
            ViewData["NoRepair"] = "";
            ViewData["RepairChecked"] = "";
            ViewData["NoRepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            ClearResult();
            InitYearTabIndex(false);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
        }

        private void LtbChanged(out bool alert)
        {
            alert = false;
            var ci = new CultureInfo("sv-SE");
            var startDate = Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci);
            LifeTimeBuy = startDate;
            var endDate = Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci);
            EndOfService = endDate;
            //var ltmp = startDate.ToString();
            //var etmp = endDate.ToString();
            if (DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, startDate, endDate) > LtbCommon.MaxServiceDays)
            {
                EndOfService = startDate.AddDays(LtbCommon.MaxServiceDays);
                //etmp = startDate.AddDays(LtbCommon.MaxServiceDays).ToString(); //(ci);
                ViewData["InfoText"] = "Fel: Serviceperioden får ej vara längre än 10 år. Vänligen ändra EoS eller LTB";
                alert = true;
            }
            else if (
                DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day,
                    Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci),
                    Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci)) < LtbCommon.MinLeadDays)
            {
                EndOfService = startDate.AddDays(LtbCommon.MinLeadDays);
                //etmp = startDate.AddDays(LtbCommon.MinLeadDays).ToString();
                ViewData["InfoText"] =
                    "Fel: Serviceperioden får ej vara längre än 10 år. Vänligen ändra EoS eller LTB";
                alert = true;
            }
            //etmp = Convert.ToDateTime(etmp, ci).ToString();
            //ltmp = Convert.ToDateTime(ltmp, ci).ToString();
            //ViewData["LTBDate"] = ltmp.Substring(0, ltmp.IndexOf(" "));
            //ViewData["EOSDate"] = etmp.Substring(0, etmp.IndexOf(" "));
            _repairSelected = GetRepairSelected();

            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
            InitYearTabIndex(_repairSelected);
        }

        private void EosChanged(out bool alert)
        {
            alert = false;

            var ci = new CultureInfo("sv-SE");
            var startDate = Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci);
            LifeTimeBuy = startDate;
            var endDate = Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci);
            EndOfService = endDate;
            //var ltmp = startDate.ToString(ci);
            //var etmp = endDate.ToString(ci);
            if (DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day, startDate, endDate) > LtbCommon.MaxServiceDays)
            {
                LifeTimeBuy = endDate.AddDays(-LtbCommon.MaxServiceDays);
                //ltmp = endDate.AddDays(-LtbCommon.MaxServiceDays).ToString(ci); //(ci);
                ViewData["InfoText"] = "Fel: Serviceperioden får ej vara längre än 10 år. Vänligen ändra EoS eller LTB";
                alert = true;
            }
            else if (
                DateTimeUtil.DateDiff(DateTimeUtil.DateInterval.Day,
                    Convert.ToDateTime(ViewData["LTBDate"].ToString(), ci),
                    Convert.ToDateTime(ViewData["EOSDate"].ToString(), ci)) < LtbCommon.MinLeadDays)
            {
                LifeTimeBuy = endDate.AddDays(-LtbCommon.MinLeadDays);
                //ltmp = endDate.AddDays(-LtbCommon.MinLeadDays).ToString(ci);
                ViewData["InfoText"] =
                    "Fel: Serviceperioden får ej vara längre än 10 år. Vänligen ändra EoS eller LTB";
                alert = true;
            }
            //etmp = Convert.ToDateTime(etmp, ci).ToString();
            //ltmp = Convert.ToDateTime(ltmp, ci).ToString();
            //ViewData["LTBDate"] = ltmp.Substring(0, ltmp.IndexOf(" "));
            //ViewData["EOSDate"] = etmp.Substring(0, etmp.IndexOf(" "));
            _repairSelected = GetRepairSelected();

            ViewData["LTBDate"] = LifeTimeBuy.ToString("d", ci);
            ViewData["EOSDate"] = EndOfService.ToString("d", ci);
            InitYearTabIndex(_repairSelected);
        }

        private void LtbDateen(string confidenceLevelInPercent, string RLD, string LTBMM, string LTBDD, string LTBYY,
            string EOSMM, string EOSDD, string EOSYY, string IB0, string IB1, string IB2, string IB3, string IB4,
            string IB5, string IB6, string IB7, string IB8, string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTBMM + '/' + LTBDD + '/' + LTBYY,
                EOSMM + '/' + EOSDD + '/' + EOSYY, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8, IB9, IB10, RS0, RS1, RS2,
                RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();
            var al = false;
            LtbChanged(out al);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (al)
            {
               // ViewData["ErrorAlert"] = "ErrorAlert";
            }
        }

        private void EosDateen(string confidenceLevelInPercent, string RLD, string LTBMM, string LTBDD, string LTBYY,
            string EOSMM, string EOSDD, string EOSYY, string IB0, string IB1, string IB2, string IB3, string IB4,
            string IB5, string IB6, string IB7, string IB8, string IB9, string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTBMM + '/' + LTBDD + '/' + LTBYY,
                EOSMM + '/' + EOSDD + '/' + EOSYY, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8, IB9, IB10, RS0, RS1, RS2,
                RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();
            var al = false;
            EosChanged(out al);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (al)
            {
                //ViewData["ErrorAlert"] = "ErrorAlert";
            }
            ;
        }

        private void LtbDatesv(string confidenceLevelInPercent, string RLD, string LTB, string EOS, string IB0,
            string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8, string IB9,
            string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTB, EOS, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8,
                IB9, IB10, RS0, RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();
            var al = false;
            LtbChanged(out al);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (al)
            {
                //ViewData["ErrorAlert"] = "ErrorAlert";
            }
        }

        private void EosDatesv(string confidenceLevelInPercent, string RLD, string LTB, string EOS, string IB0,
            string IB1, string IB2, string IB3, string IB4, string IB5, string IB6, string IB7, string IB8, string IB9,
            string IB10,
            string RS0, string RS1, string RS2, string RS3, string RS4, string RS5, string RS6, string RS7, string RS8,
            string RS9,
            string FR0, string FR1, string FR2, string FR3, string FR4, string FR5, string FR6, string FR7, string FR8,
            string FR9,
            string RL0, string RL1, string RL2, string RL3, string RL4, string RL5, string RL6, string RL7, string RL8,
            string RL9)
        {
            CopyInputToViewData(confidenceLevelInPercent, RLD, LTB, EOS, IB0, IB1, IB2, IB3, IB4, IB5, IB6, IB7, IB8,
                IB9, IB10, RS0, RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9,
                FR0, FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, RL0, RL1, RL2, RL3, RL4, RL5, RL6, RL7, RL8, RL9);
            ClearResult();

            var al = false;
            EosChanged(out al);
            ViewBag.SetFocus = "IB0";
            InitConfidenceDropDown_CheckUI();
            if (al)
            {
                //ViewData["ErrorAlert"] = "ErrorAlert";
            }
        }

        private void SetPageDefaultValues()
        {
            _repairSelected = false;
            ClearResult();
            SetDefault_LTB_EOSDate();
            ViewData["ConfLevelInPercent"] = "60";
            ViewData["RepairLeadDays"] = "40";
            ViewData["Repair"] = "javascript:SetRepair();";
            ViewData["NoRepair"] = "";
            ViewData["RepairChecked"] = "";
            ViewData["NoRepairChecked"] = new System.Web.Mvc.MvcHtmlString(string.Format("checked={0}", "\"checked\""));
            AdjustForecolorAndClearRemains(0);
            InitYearTabIndex(false);
            InitConfidenceDropDown_CheckUI();
        }

        private void CheckErrorAlert()
        {
            //ViewData["ErrorAlert"] = " ";
        }

        private static bool IsLeapYear(long year)
        {
            return (year > 0) && (year%4) == 0 && !((year%100) == 0 && (year%400) != 0);
        }

        private static long CountLeaps(long year)
        {
            return (year - 1)/4 - (year - 1)/100 + (year - 1)/400;
        }
    }
}