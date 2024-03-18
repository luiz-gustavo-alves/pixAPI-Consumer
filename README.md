# Pix API Consumer

Aplicação externa para realizar a comunicação para as PSPs (destino e origem) e processamento do pagamento das requisições feitas pela Pix API.
- No caso de sucesso, atualizar o pagamento para `SUCCESS`.
- No caso de falha, atualizar o pagamento para `FAILED`.

## Intruções para Subir os Containers da Aplicação
- Com Docker iniciado, utilize o comando `docker compose up -d` para iniciar o container do rabbitMQ.

## Instruções para Executar o Projeto
- Clone este repositório com o comando `git clone https://github.com/luiz-gustavo-alves/pixAPI-consumer.git`;
- Com o container do rabbitMQ inciado, utilize o comando `dotnet run` para subir a aplicação.

## Escopo (DTO) da Mensagem
```c#
public class PaymentMessageServiceDTO
{
  public required long Id { get; set; }
  public required string Status { get; set; }
  public required string Token { get; set; }
  public required MakePaymentDTO DTO { get; set; }
}
```

## Tratamento de Erros
- Mensagem que não pertence o mesmo escopo do DTO especificado:
  - Reject automático da mensagem.

<br>

- Mensagem que não possui o timestamp "time-to-live" no cabeçalho (header):
  - Reject automático da mensagem.
 
<br>

- API ou PSP indisponíveis:
  - Reject e novo Publish da mensagem na fila.
