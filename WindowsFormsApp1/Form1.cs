using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                    var apiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg";
                    var response = await client.PostAsync(
                        $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                        content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        string uid = result.localId;

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
                                    Form3 form3 = new Form3(uid); // ✅ Pass UID
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
                        MessageBox.Show($"Login failed: {error.error.message}");
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
