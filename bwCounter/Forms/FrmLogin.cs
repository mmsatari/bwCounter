using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace bwCounter
{
	public partial class FrmLogin : Form
	{
		private readonly WebClientEx _client;
		private string _eventvalidation = "";
		private FrmStatus _frmStatus;
		private string _viewstate = "";
		private string _viewstategenerator = "";

		public FrmLogin()
		{
			InitializeComponent();
			_client = new WebClientEx();

			LoadSettings();
			LoadLoginForm();
		}

		internal void LoadLoginForm()
		{
			try
			{
				Log("در حال تماس با سایت صبانت");
				string response = _client.DownloadString(Constants.LoginUrl);
				var doc = new HtmlDocument();
				doc.LoadHtml(response);
				Log("تماس موفقیت آمیز");
				Log("در حال گرفتن اطلاعات ورود به سایت");
				string captcha = doc.GetElementbyId("RadCaptcha1_CaptchaImage")
					.GetAttributeValue("src", "")
					.Replace("&amp;", "&");

				_viewstate = doc.GetElementbyId("__VIEWSTATE")
					.GetAttributeValue("value", "")
					.Replace("&amp;", "&");

				_viewstategenerator = doc.GetElementbyId("__VIEWSTATEGENERATOR")
					.GetAttributeValue("value", "")
					.Replace("&amp;", "&");

				_eventvalidation = doc.GetElementbyId("__EVENTVALIDATION")
					.GetAttributeValue("value", "")
					.Replace("&amp;", "&");

				pictureBox1.ImageLocation = Constants.BaseUrl + captcha;
				Log("اطلاعات گرفته شد");
			}
			catch (Exception ex)
			{
				Log(ex.Message);
				MessageBox.Show("تماس با سایت صبانت موفقیت آمیز نبود");
			}
		}

		internal void Log(string msg)
		{
			txtLog.Text += msg + Environment.NewLine;
		}

		private void btnSend_Click(object sender, EventArgs e)
		{
			Cookie cookie = Login();
			SaveSettings(txtUser.Text, txtPass.Text);
			Hide();
			_frmStatus = new FrmStatus(cookie, this);
			_frmStatus.Show();
		}

		private Cookie Login()
		{
			try
			{
				Log("در حال ارسال اطلاعات به سایت");
				var values = new NameValueCollection
				{
					{"__EVENTTARGET", ""},
					{"__EVENTARGUMENT", ""},
					{"__VIEWSTATE", _viewstate},
					{"__VIEWSTATEGENERATOR", _viewstategenerator},
					{"__EVENTVALIDATION", _eventvalidation},
					{"txtUName", txtUser.Text},
					{"txtPass", txtPass.Text},
					{"RadCaptcha1$CaptchaTextBox", txtCaptcha.Text},
					{"RadCaptcha1_ClientState", ""},
					{"btnOK.x", "10"},
					{"btnOK.y", "10"}
				};

				//_client.Headers.Add(HttpRequestHeader.Referer, baseUrl);
				_client.UploadValues(Constants.LoginUrl, "POST", values);
				var response = _client.DownloadString(Constants.HomeUrl);
				Log("موفقیت");
				var doc = new HtmlDocument();
				doc.LoadHtml(response);
				Cookie cookie = _client.CookieContainer.GetCookies(new Uri(Constants.BaseUrl))[0];
				return cookie;
			}
			catch (Exception ex)
			{
				Log(ex.Message);
				MessageBox.Show(" خطا در لاگین");
				return null;
			}
		}

		private void SaveSettings(string username, string password)
		{
			try
			{
				Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

				config.AppSettings.Settings["username"].Value = username;
				config.AppSettings.Settings["password"].Value = password;

				config.Save(ConfigurationSaveMode.Modified);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "خطا در ذخیره سازی پسورد");
			}
		}

		private void LoadSettings()
		{
			try
			{
				txtUser.Text = ConfigurationManager.AppSettings.Get("username");
				txtPass.Text = ConfigurationManager.AppSettings.Get("password");
			}
			catch (Exception ex)
			{
				//TODO if configuration does not exist => create/ if key doesn't exist => add
			}

		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void aboutMeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("http://about.me/satari");
		}

		private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
		{
			if (_frmStatus == null)
				Visible ^= true;
			else
			{
				_frmStatus.Visible ^= true;
			}
		}

		private void pictureBox2_Click(object sender, EventArgs e)
		{
			LoadLoginForm();
		}
	}
}