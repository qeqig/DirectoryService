using CSharpFunctionalExtensions;
using Shared;

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

   public static Result<PositionName, Error> Create(string value)
   {
       if (string.IsNullOrWhiteSpace(value))
           return GeneralErrors.ValueIsRequired("position name");

       if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
           return GeneralErrors.ValueIsInvalid("position name");

       return new PositionName(value);
   }
}