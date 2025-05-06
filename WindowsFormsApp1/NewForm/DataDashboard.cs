using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1.NewForm
{
    public partial class DataDashboard : UserControl
    {
        // Replace these with your actual Firebase project ID and Web API key
        private const string FirebaseProjectId = "tuluyan-user-login";
        private const string ApiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg";

        private static readonly HttpClient client = new HttpClient();

        public DataDashboard()
        {
            InitializeComponent();
            DisplayDateAndTime();
            _ = DisplayCounts(); // Trigger async method without awaiting in constructor
        }

        private void DisplayDateAndTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime philippinesDateTime = utcNow.AddHours(8);
            label4.Text = philippinesDateTime.ToString("MMMM dd, yyyy");
            label5.Text = philippinesDateTime.ToString("hh:mm:ss tt");
        }

        public async Task DisplayCounts()
        {
            try
            {
                // Get your data counts
                int tenantApprovedCount = await GetApprovedCountFromCollection(
                    "TenantCollection",
                    "RentedBoardingHouse",
                    "status",
                    "Approved");

                int landlordApprovedCount = await GetLandlordApprovedCount();

                // Clear existing points
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series2"].Points.Clear();

                // Add two points to each series to create the spline line
                // Point 1: Starting at 0
                chart1.Series["Series1"].Points.AddXY(0, 0);
                chart1.Series["Series2"].Points.AddXY(0, 0);

                // Point 2: Actual value
                chart1.Series["Series1"].Points.AddXY(1, landlordApprovedCount);
                chart1.Series["Series2"].Points.AddXY(1, tenantApprovedCount);

                // Configure X-axis
                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = 1;
                chart1.ChartAreas[0].AxisX.Interval = 1;
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;

                // Configure Y-axis
                int maxCount = Math.Max(landlordApprovedCount, tenantApprovedCount);
                chart1.ChartAreas[0].AxisY.Minimum = 0;
                chart1.ChartAreas[0].AxisY.Maximum = maxCount + (maxCount > 0 ? 1 : 0);
                chart1.ChartAreas[0].AxisY.Interval = 1;

                // Refresh chart
                chart1.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating chart: " + ex.Message);
            }
        }


        private async Task<int> GetApprovedCountFromCollection(string collection, string documentField, string statusField, string targetStatus)
        {
            int count = 0;
            string url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{collection}?key={ApiKey}";

            try
            {
                var response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);

                if (json["documents"] != null)
                {
                    foreach (var doc in json["documents"])
                    {
                        // Check if this is a RentedBoardingHouse document
                        if (doc["fields"]?["title"]?["stringValue"]?.ToString() == documentField)
                        {
                            string status = doc["fields"]?[statusField]?["stringValue"]?.ToString();
                            if (status == targetStatus)
                            {
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data from {collection}: " + ex.Message);
            }

            return count;
        }

        private async Task<int> GetLandlordApprovedCount()
        {
            int totalApproved = 0;
            string landlordCollectionUrl = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/LandlordCollection?key={ApiKey}";

            try
            {
                var landlordResponse = await client.GetStringAsync(landlordCollectionUrl);
                JObject landlordJson = JObject.Parse(landlordResponse);

                if (landlordJson["documents"] != null)
                {
                    foreach (var landlordDoc in landlordJson["documents"])
                    {
                        string docName = landlordDoc["name"]?.ToString();
                        string[] segments = docName.Split('/');
                        string landlordId = segments[segments.Length - 1];

                        string boardingHousesUrl = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/LandlordCollection/{landlordId}/BoardingHouses?key={ApiKey}";

                        var boardingHousesResponse = await client.GetStringAsync(boardingHousesUrl);
                        JObject boardingHousesJson = JObject.Parse(boardingHousesResponse);

                        if (boardingHousesJson["documents"] != null)
                        {
                            foreach (var boardingHouseDoc in boardingHousesJson["documents"])
                            {
                                string boardingHouseName = boardingHouseDoc["name"]?.ToString();
                                string[] bhSegments = boardingHouseName.Split('/');
                                string boardingHouseId = bhSegments[bhSegments.Length - 1];

                                string occupantsUrl = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/LandlordCollection/{landlordId}/BoardingHouses/{boardingHouseId}/Occupants?key={ApiKey}";

                                var occupantsResponse = await client.GetStringAsync(occupantsUrl);
                                JObject occupantsJson = JObject.Parse(occupantsResponse);

                                if (occupantsJson["documents"] != null)
                                {
                                    foreach (var occupantDoc in occupantsJson["documents"])
                                    {
                                        string status = occupantDoc["fields"]?["status"]?["stringValue"]?.ToString();
                                        if (status == "Approved")
                                        {
                                            totalApproved++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving landlord data: " + ex.Message);
            }

            return totalApproved;
        }

    }
}
