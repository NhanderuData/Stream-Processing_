using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

Console.Write("Digite o IP ou Domínio (ex: scanme.nmap.org): ");
string alvo = Console.ReadLine() ?? "scanme.nmap.org";

int[] portas = { 21, 22, 25, 80, 110, 443, 3306, 8080 };

Console.WriteLine($"\nIniciando varredura em {alvo}...\n");
foreach (int porta in portas)
{
    await CapturarBannerAsync(alvo, porta);
}

Console.WriteLine("\nScan concluído.");
async Task CapturarBannerAsync(string host, int port)
{
    try
    {
        using TcpClient cliente = new TcpClient();

        var tarefaConexao = cliente.ConnectAsync(host, port);
        if (await Task.WhenAny(tarefaConexao, Task.Delay(2000)) != tarefaConexao)
        {
            Console.WriteLine($"[-] Porta {port} -> Timeout (Filtrada por Firewall ou Host Offline)");
            return;
        }

        using NetworkStream stream = cliente.GetStream();

        if (port == 80 || port == 443)
        {
            string requisicao = $"HEAD / HTTP/1.1\r\nHost: {host}\r\n\r\n";
            byte[] bytesRequisicao = Encoding.ASCII.GetBytes(requisicao);
            await stream.WriteAsync(bytesRequisicao, 0, bytesRequisicao.Length);
        }

        byte[] buffer = new byte[1024];

        stream.ReadTimeout = 2000;

        int bytesLidos = await stream.ReadAsync(buffer, 0, buffer.Length);

        if (bytesLidos > 0)
        {
            string banner = Encoding.ASCII.GetString(buffer, 0, bytesLidos).Trim().Replace("\r", "").Replace("\n", " ");
            int tamanhoExibicao = Math.Min(banner.Length, 80);

            Console.WriteLine($"[+] Porta {port} ABERTA | Banner: {banner.Substring(0, tamanhoExibicao)}...");
        }
        else
        {
            Console.WriteLine($"[+] Porta {port} ABERTA | Nenhum banner retornado.");
        }
    }
    catch (Exception)
    {
        Console.WriteLine($"[-] Porta {port} -> Fechada (Conexão Recusada)");
    }
}