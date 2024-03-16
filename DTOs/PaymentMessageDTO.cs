namespace consumer.DTOs;

public class PaymentMessageServiceDTO
{
  public required long Id { get; set; }
  public required string Status { get; set; }

  public required string Token { get; set; }
  public required TransferRequestDTO DTO { get; set; } = null!;
}

public class TransferRequestDTO
{
  public required OriginDTO Origin { get; set; } = null!;
  public required DestinyDTO Destiny { get; set; } = null!;
  public int Amount { get; set; }
  public string? Description { get; set; } = null!;
}

public class OriginDTO
{
  public required UserDTO User { get; set; } = null!;
  public required AccountDTO Account { get; set; } = null!;
}

public class UserDTO
{
  public required string CPF { get; set; } = null!;
}

public class AccountDTO
{
  public required string Number { get; set; } = null!;
  public required string Agency { get; set; } = null!;
}

public class DestinyDTO
{
  public required KeyDTO Key { get; set; } = null!;
}

public class KeyDTO
{
  public required string Value { get; set; } = null!;
  public required string Type { get; set; } = null!;
}