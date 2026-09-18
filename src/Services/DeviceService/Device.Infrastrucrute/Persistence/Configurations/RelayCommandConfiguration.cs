namespace Device.Infrastructure.Persistence.Configurations;

public sealed class RelayCommandConfiguration : IEntityTypeConfiguration<RelayCommand>
{
    public void Configure(EntityTypeBuilder<RelayCommand> builder)
    {
        builder.ToTable("relay_command_queues");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ControllerId).IsRequired();
        builder.Property(x => x.RelayId).IsRequired();
        builder.Property(x => x.TargetState).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ExpireAt).IsRequired(false);
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new
        {
            x.ControllerId,
            x.Status
        });

        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
