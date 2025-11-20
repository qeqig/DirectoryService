using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Position.VO;

public record PositionName
{
   public const int MIN_LENGTH = 3;

   public const int MAX_LENGTH = 100;

   private PositionName(string value)
   {
       Value = value;
   }

   public string Value { get; }

   public static Result<PositionName> Create(string value)
   {
       if (string.IsNullOrWhiteSpace(value))
           return Result.Failure<PositionName>("PositionName cannot be empty");

       if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
           return Result.Failure<PositionName>("The position name is too short or too long");

       return new PositionName(value);
   }
}