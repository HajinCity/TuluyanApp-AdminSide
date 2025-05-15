using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.NewForm;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private string _email;  // Store the email of the logged-in user
        private string _uid;    // Store the UID of the logged-in user

        // Constructor to accept both email and uid
        public Form2(string email, string uid)
        {
            InitializeComponent();
            _email = email;  // Initialize the email
            _uid = uid;      // Initialize the UID
            LoadUsersToDataGridView(); // Load Firestore data on form load
        }

        // Create a new user account upon clicking the button
        private async void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.Trim();
            string password = textBox2.Text;
            string confirmPassword = textBox3.Text;
            string name = textBox4.Text.Trim();

            // Check if any required fields are empty
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            try
            {
                // Call Firebase Authentication API to create the user
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

                    var apiKey = "AIzaSyDcGUMEFwKVWV29kD3yBCS4TGOnboaIKRg"; // Firebase Web API key
                    var response = await client.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Check if the response was successful
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        string uid = result.localId;

                        // Now, save the user data to Firestore
                        try
                        {
                            DocumentReference docRef = FirebaseInitialization.Database.Collection("users").Document(uid);
                            Dictionary<string, object> userData = new Dictionary<string, object>
                            {
                                { "email", email },
                                { "name", name },
                                { "role", "admin" },  // Default to admin role
                                { "createdAt", Timestamp.GetCurrentTimestamp() }
                            };

                            await docRef.SetAsync(userData); // Save user data to Firestore
                            MessageBox.Show("Account created and saved to Firestore!");

                            // Clear the textboxes after success
                            textBox1.Clear();
                            textBox2.Clear();
                            textBox3.Clear();
                            textBox4.Clear();

                            // Refresh the DataGridView
                            LoadUsersToDataGridView();
                        }
                        catch (Exception firestoreEx)
                        {
                            MessageBox.Show("Account created, but Firestore error: " + firestoreEx.Message);
                        }
                    }
                    else
                    {
                        // Display Firebase Authentication error
                        dynamic error = JsonConvert.DeserializeObject(responseString);
                        MessageBox.Show($"Error: {error.error.message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        // Load all users from Firestore into DataGridView
        private async void LoadUsersToDataGridView()
        {
            try
            {
                QuerySnapshot snapshot = await FirebaseInitialization.Database.Collection("users").GetSnapshotAsync();
                dataGridView1.Rows.Clear();

                // Populate the DataGridView with user data from Firestore
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();
                    string name = data.ContainsKey("name") ? data["name"].ToString() : "";
                    string email = data.ContainsKey("email") ? data["email"].ToString() : "";
                    string role = data.ContainsKey("role") ? data["role"].ToString() : "";
                    string createdAt = "";

                    if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
                    {
                        createdAt = ts.ToDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    // Add the user data to the DataGridView
                    dataGridView1.Rows.Add(name, email, role, createdAt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Firestore data: " + ex.Message);
            }
        }

        // Open the password change form when button2 is clicked
        private void button2_Click(object sender, EventArgs e)
        {
            // Assuming _uid contains the current user's UID and _email contains the current user's email
            changepass changePasswordForm = new changepass(_email);  // Pass the email to the changepass form
            changePasswordForm.ShowDialog();  // Open the password change dialog
        }


    }
}
