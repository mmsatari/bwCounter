using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace bwCounter
{
	public partial class FrmStatus : Form
	{
		private readonly Cookie _cookie;
		private readonly FrmLogin _frmLogin;
		private string _remainingDownloadCapacity;

		public FrmStatus(Cookie cookie, FrmLogin frmLogin)
		{
			InitializeComponent();
			_cookie = cookie;
			_frmLogin = frmLogin;
		}

		private void frmStatus_Load(object sender, EventArgs e)
		{
			var worker = new BackgroundWorker();
			worker.DoWork += worker_DoWork;
			try
			{
				timer1.Interval = int.Parse(ConfigurationManager.AppSettings.Get("interval"))*Constants.Minute;
			}
			catch
			{
				timer1.Interval = 10*Constants.Minute;
				SaveSetting("interval", timer1.Interval.ToString());
			}
			GetStats();
			lblUpdate.Visible = false;
		}

		private void SaveSetting(string key, string value)
		{
			try
			{
				Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

				config.AppSettings.Settings[key].Value = value;
				config.Save(ConfigurationSaveMode.Modified);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "خطا در ذخیره سازی ");
			}
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			lblUpdate.Visible = true;
			GetStats();
			lblUpdate.Visible = false;
		}

		private void GetStats()
		{
			try
			{
				var request = WebRequest.Create(Constants.HomeUrl) as HttpWebRequest;
				request.CookieContainer = new CookieContainer();
				request.CookieContainer.Add(_cookie);

				WebResponse resp = request.GetResponse();
				if (((HttpWebResponse) resp).StatusCode != HttpStatusCode.OK)
				{
					throw new Exception("شما از سایت خارج شده اید. لطفا دوباره لاگین کنید!");
				}
				Stream stream = resp.GetResponseStream();
				TextReader myTextReader = new StreamReader(stream);
				string html = myTextReader.ReadToEnd();

				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				_remainingDownloadCapacity = doc.GetElementbyId("ctl00_ContentPlaceHolder1_lblRemainedCredit").InnerText.Trim();
				label2.Text = string.Format("{0} {1}", _remainingDownloadCapacity.Split(' ')[1], _remainingDownloadCapacity.Split(' ')[0]);
				_frmLogin.notifyIcon1.Text = string.Format("{1} حجم باقیمانده:{0} ", _remainingDownloadCapacity.Split(' ')[1], _remainingDownloadCapacity.Split(' ')[0]);
				lblLastUpdate.Text = DateTime.Now.ToShortTimeString();
			}
			catch (Exception exception)
			{
				timer1.Enabled = false;
				Visible = true;
				MessageBox.Show(exception.Message, "خطا");
				_frmLogin.Log(exception.Message);
				_frmLogin.LoadLoginForm();
				_frmLogin.Visible = true;
				Close();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			worker_DoWork(sender, new DoWorkEventArgs(null));
		}

		private void frmStatus_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			Visible ^= true;
		}

		private void frmStatus_MouseClick(object sender, MouseEventArgs e)
		{
			lblUpdate.Visible = true;
			GetStats();
			lblUpdate.Visible = false;
		}
	}
}