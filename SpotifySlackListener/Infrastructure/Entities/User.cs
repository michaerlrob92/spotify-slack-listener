using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpotifySlackListener.Infrastructure.Entities
{
    public class User
    {
        public string Id { get; set; }

        public string SlackId { get; set; }
        
        public string SlackAccessToken { get; set; }
        
        public string SlackTeamId { get; set; }

        public string SpotifyId { get; set; }

        public string SpotifyAccessToken { get; set; }
        
        public string SpotifyRefreshToken { get; set; }
        
        public string CurrentStatus { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime LastUpdated { get; set; }
    }

    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasMaxLength(128);

            builder.Property(u => u.SlackId)
                .HasMaxLength(256)
                .IsRequired();
            builder.Property(u => u.SlackAccessToken)
                .HasMaxLength(256)
                .IsRequired();
            builder.Property(u => u.SlackTeamId)
                .HasMaxLength(256);
            builder.Property(u => u.SpotifyId)
                .HasMaxLength(256)
                .IsRequired();
            builder.Property(u => u.SpotifyAccessToken)
                .HasMaxLength(256)
                .IsRequired();
            builder.Property(u => u.SpotifyRefreshToken)
                .HasMaxLength(256);

            builder.HasIndex(u => new {u.SpotifyAccessToken, u.SlackAccessToken});
            builder.HasIndex(u => new {u.SpotifyId, u.SlackId});
        }
    }
}