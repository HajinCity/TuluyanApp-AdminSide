using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.NewForm;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        private string _email; // Store email of the user
        private string _uid;   // Store UID of the user
        private Form activeForm;

        // Constructor to accept email and UID
        public Form3(string email, string uid)
        {
            InitializeComponent();
            _email = email;
            _uid = uid;

            // Fire and forget async task to load user data
            LoadUserName();
            home_btn_Click(null, EventArgs.Empty); // Set the default page to Home
        }

        // Async method to load user data from Firestore based on UID
        private async void LoadUserName()
        {
            try
            {
                DocumentSnapshot userDoc = await FirebaseInitialization.Database
                    .Collection("users")
                    .Document(_uid)
                    .GetSnapshotAsync();

                if (userDoc.Exists && userDoc.TryGetValue("name", out string name))
                {
                    label2.Text = name; // Set the name in label2
                }
                else
                {
                    label2.Text = "Name not found";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load user name: " + ex.Message);
            }
        }

        // Method to open and manage child forms within the panel
        private void openingForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();

            activeForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panel3.Controls.Add(childForm);
            panel3.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // Home button click - opens the HomeDashboard form
        private void home_btn_Click(object sender, EventArgs e)
        {
            openingForm(new HomeDashboard());
        }

        // Management button click - opens Form2, passing email and UID
        private void management_btn_Click(object sender, EventArgs e)
        {
            openingForm(new Form2(_email, _uid)); // Pass email and UID to Form2
        }

        // Logout button click - confirms and logs the user out
        private void logout_btn_Click(object sender, EventArgs e)
        {
            // Optional: Confirm logout
            var confirm = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                this.Hide(); // Hide the current Form3
                Form1 loginForm = new Form1(); // Create a new instance of Form1
                loginForm.Show(); // Show Form1 (login screen)
            }
        }
    }
}
