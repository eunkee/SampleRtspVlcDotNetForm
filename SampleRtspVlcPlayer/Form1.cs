using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleRtspVlcPlayer
{
    public partial class Form1 : Form
    {
        private readonly Vlc.DotNet.Forms.VlcControl vlcControl0 = new Vlc.DotNet.Forms.VlcControl();
        private readonly string _rtspAddress = "rtsp://admin:123456@172.30.1.32/streaming/channel/101";
        readonly FileInfo _file = new FileInfo(@"C:\Users\user\Desktop\dd_test1.avi");
        readonly string[] _vlcOptions = {
                //":rtsp-http",
                //":rtsp-http-port=" + Convert.ToString(m_port0),
                //":rtsp-user=" + m_account0,
                //":rtsp-pwd=" + m_password0,
                ":network-caching=500",
                ":live-caching=500",
                //"--disable-libmpeg2",
                //"-aspect-ratio=4:3"
            };

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Controls.Add(vlcControl0);
            vlcControl0.Location = new Point(0, 0);
            vlcControl0.Size = new Size(800, 600);
            vlcControl0.Dock = DockStyle.Fill;
            
            vlcControl0.BackColor = System.Drawing.Color.Black;
            vlcControl0.BringToFront();

            vlcControl0.VlcLibDirectory = new System.IO.DirectoryInfo(@"C:\vlc");
            //string _exeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //vlcControl0.VlcLibDirectory = new DirectoryInfo(_exeFolder + @"\libvlc");

            vlcControl0.VlcMediaplayerOptions = new[]
            {
                "--intf",
                "dummy", 
                "--no-audio", 
                "--no-video-title-show",
                "--no-stats",
                "--no-sub-autodetect-file",
                "--no-osd",
                "--no-snapshot-preview",
                "--skip-frames",
            };



            //VLC 끝날 경우
            vlcControl0.EndReached += MediaPlayerOnEndReached0;
            vlcControl0.EndInit();

            //vlcControl0.SetMedia(new Uri(rtspAddress), vlcOptions);
            vlcControl0.SetMedia(_file, _vlcOptions);

            // 재생
            vlcControl0.Play();

            //vlcControl0.Video.AspectRatio = $"{this.Width}:{this.Height}";
            //vlcControl0.Video.IsMouseInputEnabled = false;
            //vlcControl0.Video.IsKeyInputEnabled = false;
            Vlc.DotNet.Core.IMarqueeManagement marquee = vlcControl0.Video.Marquee;
            marquee.Position = 5;
            marquee.Size = 92;
            marquee.Text = "Cam 1";
            marquee.X = 20;
            marquee.Y = 20;
            marquee.Color = Color.Orange.ToArgb();
            marquee.Enabled = true;
        }

        // 끝난 경우 재 연결
        private async void MediaPlayerOnEndReached0(object sender, EventArgs vlcMediaPlayerEndReachedEventArgs)
        {
            try
            {
                //재연결을 위해서 컨트롤 길이 수정
                await Task.Factory.StartNew(() => { vlcControl0.OnLengthChanged(0); });

                _reconnectCount = 30;
            }
            catch
            { }
        }

        private const int RECONNECT_COUNT = 100;
        private int _reconnectCount = RECONNECT_COUNT;

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if(vlcControl0 == null)
            {
                return;
            }
            if (vlcControl0.GetCurrentMedia() == null)
            {
                return;
            }

            if (vlcControl0.GetCurrentMedia().State != Vlc.DotNet.Core.Interops.Signatures.MediaStates.Playing)
            {
                //설정된 횟수에 도달할 떄 까지 감소
                _reconnectCount--;
                if (_reconnectCount <= 0)
                {
                    //감소 값 초기화
                    _reconnectCount = RECONNECT_COUNT;
                    //연결이 끊겼다고 판단할 때
                    try
                    {
                        vlcControl0.EndInit();

                        //vlcControl0.SetMedia(new Uri(rtspAddress), _vlcOptions);
                        vlcControl0.SetMedia(_file, _vlcOptions);

                        // 재생
                        vlcControl0.Play();
                    }
                    catch { }
                }
            }
        }
    }
}
