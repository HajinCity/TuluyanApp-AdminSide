using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1.NewForm
{
    public partial class DataDashboard : UserControl
    {
        private const string FirebaseProjectId = "tuluyan-user-login";
        private const string ApiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg";
        private static readonly HttpClient client = new HttpClient();
        private Timer refreshTimer;

        public DataDashboard()
        {
            InitializeComponent();
            DisplayDateAndTime();
            InitializeRefreshTimer();
            _ = LoadDataCountsAsync();
            _ = LoadOccupantDataAsync();

            // Hook up the TextChanged event for filtering
            textBox1.TextChanged += textBox1_TextChanged;
        }

        private void InitializeRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += async (sender, e) =>
            {
                DisplayDateAndTime();

                if (DateTime.Now.Second == 0)
                {
                    await LoadDataCountsAsync();
                    await LoadOccupantDataAsync();
                }
            };
            refreshTimer.Start();
        }

        private void DisplayDateAndTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime philippinesTime = utcNow.AddHours(8);
            label4.Text = philippinesTime.ToString("MMMM dd, yyyy");
            label5.Text = philippinesTime.ToString("hh:mm:ss tt");
        }

        private async Task LoadDataCountsAsync()
        {
            try
            {
                int tenantCount = await GetCollectionCount("TenantCollection");
                int landlordCount = await GetCollectionCount("LandlordCollection");

                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        label8.Text = tenantCount.ToString();
                        label9.Text = landlordCount.ToString();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data counts: {ex.Message}");
            }
        }

        private async Task<int> GetCollectionCount(string collectionName)
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{collectionName}?key={ApiKey}";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            return json["documents"]?.Count() ?? 0;
        }

        private async Task LoadOccupantDataAsync()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        dataGridView1.Rows.Clear();
                    });
                }

                var landlords = await GetCollectionData("LandlordCollection");

                foreach (var landlord in landlords)
                {
                    if (!landlord.TryGetValue("__docId", out var landlordDocIdToken))
                        continue;

                    string landlordId = landlordDocIdToken.ToString();
                    string landlordName = $"{landlord.GetValueOrDefaultSafe("FirstName")} {landlord.GetValueOrDefaultSafe("LastName")}".Trim();

                    var boardingHouses = await GetSubCollectionData("LandlordCollection", landlordId, "BoardingHouses");

                    foreach (var boardingHouse in boardingHouses)
                    {
                        if (!boardingHouse.TryGetValue("__docId", out var bhDocIdToken))
                            continue;

                        string address = boardingHouse.GetValueOrDefaultSafe("address", "No address");
                        string boardinghouseId = bhDocIdToken.ToString();

                        var occupants = await GetSubCollectionData($"LandlordCollection/{landlordId}/BoardingHouses", boardinghouseId, "Occupants");

                        foreach (var occupant in occupants)
                        {
                            try
                            {
                                string firstName = occupant.GetValueOrDefaultSafe("firstName");
                                string lastName = occupant.GetValueOrDefaultSafe("lastName");
                                string fullName = $"{firstName} {lastName}".Trim();
                                string contactNo = occupant.GetValueOrDefaultSafe("contactNo");
                                string status = occupant.GetValueOrDefaultSafe("status");

                                if (status != "Approved") continue;

                                if (this.IsHandleCreated)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        int rowIndex = dataGridView1.Rows.Add();
                                        dataGridView1.Rows[rowIndex].Cells["Column1"].Value = fullName;
                                        dataGridView1.Rows[rowIndex].Cells["Column2"].Value = address;
                                        dataGridView1.Rows[rowIndex].Cells["Column3"].Value = contactNo;
                                        dataGridView1.Rows[rowIndex].Cells["Column4"].Value = landlordName;
                                    });
                                }
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Skipping occupant due to missing data: {innerEx.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading nested occupant data: {ex.Message}");
            }
        }

        private async Task<List<Dictionary<string, JToken>>> GetSubCollectionData(string parentPath, string documentId, string subCollection)
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{parentPath}/{documentId}/{subCollection}?key={ApiKey}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var result = new List<Dictionary<string, JToken>>();

            if (json["documents"] != null)
            {
                foreach (var doc in json["documents"])
                {
                    var fields = doc["fields"]?.ToObject<Dictionary<string, JToken>>() ?? new Dictionary<string, JToken>();
                    string docName = doc["name"]?.ToString();
                    string[] parts = docName?.Split('/');
                    string docId = parts?.Last();
                    fields["__docId"] = docId;

                    result.Add(fields);
                }
            }

            return result;
        }

        private async Task<List<Dictionary<string, JToken>>> GetCollectionData(string collectionName)
        {
            string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{collectionName}?key={ApiKey}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var result = new List<Dictionary<string, JToken>>();

            if (json["documents"] != null)
            {
                foreach (var doc in json["documents"])
                {
                    var fields = doc["fields"]?.ToObject<Dictionary<string, JToken>>() ?? new Dictionary<string, JToken>();
                    string docName = doc["name"]?.ToString();
                    string[] parts = docName?.Split('/');
                    string docId = parts?.Last();
                    fields["__docId"] = docId;

                    result.Add(fields);
                }
            }

            return result;
        }

        // 🔍 Filtering logic based on textbox1 input
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text.Trim().ToLower();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                bool isVisible = false;

                for (int i = 0; i < 4; i++) // assuming Columns 0 to 3 are: FullName, Address, Contact, Landlord
                {
                    var cellValue = row.Cells[i].Value?.ToString().ToLower() ?? "";
                    if (cellValue.Contains(filterText))
                    {
                        isVisible = true;
                        break;
                    }
                }

                row.Visible = isVisible;
            }
        }
    }

    public static class DictionaryExtensions
    {
        public static string GetValueOrDefaultSafe(this Dictionary<string, JToken> dict, string key, string fallback = "")
        {
            if (dict.TryGetValue(key, out JToken value))
            {
                if (value.Type == JTokenType.Object && value["stringValue"] != null)
                {
                    return value["stringValue"]?.ToString() ?? fallback;
                }
                return value?.ToString() ?? fallback;
            }

            Console.WriteLine($"Missing key: {key}");
            return fallback;
        }
    }
}
