using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;


namespace BcxbDataAccess {

   public static class DataAccess {

      public static HttpClient client;
      public static string WinhostEndPoint;
      public static List<CTeamRecord> TeamCache = new List<CTeamRecord>();
//delete this line.

      static DataAccess() {

         // Use httpS here as I have added SSL cert to Z.com on WinHost (7/15'20)...

         // Use this for ptoduction... 
         //client = new HttpClient() { BaseAddress = new Uri("https://www.zeemerix.com") };
         //WinhostEndPoint = "liveteamrdr/";

         // Use this for test app on Winhost...
         client = new HttpClient() { BaseAddress = new Uri("https://www.zeemerix.com") };
         WinhostEndPoint = "liveteamrdr_test/";

         // Use this for localhost...
         //client = new HttpClient() { BaseAddress = new Uri("https://localhost:44389") };
         //WinhostEndPoint = "";

      }


      public static async Task<DTO_TeamRoster> GetTeamRosterOnLine(string team, int year) {
         // --------------------------------------------------------------------------------------
         var url = new Uri(client.BaseAddress, $"{WinhostEndPoint}api/team/{team}/{year}");

         client.DefaultRequestHeaders.Accept.Clear();
         client.DefaultRequestHeaders.Accept.Add(
             new MediaTypeWithQualityHeaderValue("application/json"));

         DTO_TeamRoster roster = null;
         HttpResponseMessage response = await client.GetAsync(url.ToString());
         if (response.IsSuccessStatusCode) {
            roster = await response.Content.ReadAsAsync<DTO_TeamRoster>();
         }
         else {
            roster = null;
            throw new Exception($"Error loading team {team} for {year}");
         }
         return roster;

      }


      public static async Task<List<CTeamRecord>> GetTeamListForYearOnLine(int year) {
         // --------------------------------------------------------------------------------------
         // This is not used... see GetTeamListForYearFromCache instead. -bc


         //var t = new List<CTeamRecord> {
         //   new CTeamRecord { TeamTag = "NYY2018", City = "New York", LineName = "NYY", NickName = "Yankees", UsesDh = true, LgID = "AL" },
         //   new CTeamRecord { TeamTag = "NYM2018", City = "New York", LineName = "NYM", NickName = "Mets", UsesDh = false, LgID = "NL" },
         //   new CTeamRecord { TeamTag = "BOS2015", City = "Boston", LineName = "Bos", NickName = "Red Sox", UsesDh = true, LgID = "AL" },
         //   new CTeamRecord { TeamTag = "PHI2015", City = "Philadelphia", LineName = "Phi", NickName = "Phillies", UsesDh = false, LgID = "NL" },
         //   new CTeamRecord { TeamTag = "WAS2019", City = "Washington", LineName = "Was", NickName = "Nationals", UsesDh = false, LgID = "NL" }
         //};
         //return t;

         // Right here I could have logic that maintains a master list and refreshes by 10-year ranges.

         var url = new Uri(client.BaseAddress, $"{WinhostEndPoint}api/team-list/{year}/{year}");

         client.DefaultRequestHeaders.Accept.Clear();
         client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

         List<CTeamRecord> teamList = null;
         HttpResponseMessage response = await client.GetAsync(url.ToString());
         if (response.IsSuccessStatusCode) {
            teamList = await response.Content.ReadAsAsync<List<CTeamRecord>>();
         }
         else {
            teamList = null;
            throw new Exception($"Error loading list of teams for {year}\r\nStatus code: {response.StatusCode}"); // 2.0.01
         }
         return teamList;


      }


      public static async Task<List<CTeamRecord>> GetCustTeamListForUser(string userName) {
         // --------------------------------------------------------------------------------------

         //var t = new List<CTeamRecord> {
         //   new CTeamRecord { TeamTag = "NYY2018", City = "New York", LineName = "NYY", NickName = "Yankees", UsesDh = true, LgID = "AL" },
         //   new CTeamRecord { TeamTag = "NYM2018", City = "New York", LineName = "NYM", NickName = "Mets", UsesDh = false, LgID = "NL" },
         //   new CTeamRecord { TeamTag = "BOS2015", City = "Boston", LineName = "Bos", NickName = "Red Sox", UsesDh = true, LgID = "AL" },
         //   new CTeamRecord { TeamTag = "PHI2015", City = "Philadelphia", LineName = "Phi", NickName = "Phillies", UsesDh = false, LgID = "NL" },
         //   new CTeamRecord { TeamTag = "WAS2019", City = "Washington", LineName = "Was", NickName = "Nationals", UsesDh = false, LgID = "NL" }
         //};
         //return t;

         var url = new Uri(client.BaseAddress, $"{WinhostEndPoint}api/team-list-cust/{userName}");

         client.DefaultRequestHeaders.Accept.Clear();
         client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

         List<CTeamRecord> teamList = null;
         HttpResponseMessage response = await client.GetAsync(url.ToString());
         if (response.IsSuccessStatusCode) {
            teamList = await response.Content.ReadAsAsync<List<CTeamRecord>>();
         }
         else {
            teamList = null;
            throw new Exception($"Error loading list of custom teams for {userName}\r\nStatus code: {response.StatusCode}"); 
         }
         return teamList;

      }


      public static async Task<List<CTeamRecord>> GetTeamListForYearFromCache(int year) {
         // --------------------------------------------------------------------------------------
         List<CTeamRecord> result;
         Debug.WriteLine($"At start of GetTeamListForYearFromCache: {TeamCache.Count} in TeamCache"); //#3000.04

         result = TeamCache.Where(t => t.Year == year).ToList();
         if (result.Count > 0) {
            result.Insert(0, new CTeamRecord { TeamTag = "", Year = 0 });
            return result;
         }
         else {
            // The year is not in the teamCache, 
            // so, fetch 10 year block from DB and add to cache...
            int year1 = 10 * (year / 10);
            int year2 = year1 + 9;
            var url = new Uri(client.BaseAddress, $"{WinhostEndPoint}api/team-list/{year1}/{year2}");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                  new MediaTypeWithQualityHeaderValue("application/json"));

            List<CTeamRecord> yearList10;
            HttpResponseMessage response = await client.GetAsync(url.ToString());
            if (response.IsSuccessStatusCode) {
               yearList10 = await response.Content.ReadAsAsync<List<CTeamRecord>>();
            }
            else {
               yearList10 = null;
               throw new Exception($"Error loading list of teams for {year}\r\nStatus code: {response.StatusCode}");
            }
            TeamCache.AddRange(yearList10);

            result = TeamCache.Where(t => t.Year == year).ToList();
            Debug.WriteLine($"Returning from GetTeamListForYearFromCache: {TeamCache.Count} in TeamCache"); //#3000.04
            result.Insert(0, new CTeamRecord { TeamTag = "", Year = 0 });

            return result;

         }

      }


      public static async Task<DTO_TeamRoster> GetCustTeamRoster(int teamID) {
     // --------------------------------------------------------------------------------------
         var url = new Uri(client.BaseAddress, $"{WinhostEndPoint}api/team-cust/{teamID}");

         client.DefaultRequestHeaders.Accept.Clear();
         client.DefaultRequestHeaders.Accept.Add(
             new MediaTypeWithQualityHeaderValue("application/json"));

         DTO_TeamRoster roster = null;
         HttpResponseMessage response = await client.GetAsync(url.ToString());
         if (response.IsSuccessStatusCode) {
            roster = await response.Content.ReadAsAsync<DTO_TeamRoster>();
         }
         else {
            roster = null;
            throw new Exception($"Error loading custom team {teamID}");
         }
         return roster;

      }


      public static void ClearTeamCache() {
         // ----------------------------------------------------------
         TeamCache.Clear();

      }

   }


}


