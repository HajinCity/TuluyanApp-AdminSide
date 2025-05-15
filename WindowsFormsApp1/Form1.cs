using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Cloud.Firestore;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both email and password.");
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new
                    {
                        email = email,
                        password = password,
                        returnSecureToken = true
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var apiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg"; // Replace with your real Firebase Web API key
                    var response = await client.PostAsync(
                        $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                        content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        string uid = result.localId;
                        string idToken = result.idToken;

                        var userDoc = await FirebaseInitialization.Database
                            .Collection("users")
                            .Document(uid)
                            .GetSnapshotAsync();

                        if (userDoc.Exists)
                        {
                            if (userDoc.TryGetValue("role", out string role))
                            {
                                if (role == "admin")
                                {
                                    MessageBox.Show("Login successful!");

                                    // Pass the email and uid to Form3
                                    Form3 form3 = new Form3(email, uid); // Pass email and uid
                                    form3.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("You are not authorized as an admin.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Role not found in Firestore.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("User not found in Firestore.");
                        }
                    }
                    else
                    {
                        dynamic error = JsonConvert.DeserializeObject(responseString);
                        string errorMessage = error?.error?.message ?? "Unknown error occurred during login.";
                        MessageBox.Show($"Login failed: {errorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }
}
