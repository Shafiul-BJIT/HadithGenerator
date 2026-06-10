using HadithGenerator.Constants;

namespace HadithGenerator.Models;

public abstract class BaseModel
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; private set; } = HGTime.Now;
    public DateTimeOffset UpdatedAt { get; private set; } = HGTime.Now;
    public bool IsDeleted { get; private set; } = false;

    public void MarkUpdated() => UpdatedAt = HGTime.Now;

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = HGTime.Now;
    }
}