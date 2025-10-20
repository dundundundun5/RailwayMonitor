using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RailwayAlarmBackend.Models.Entities;

namespace RailwayAlarmBackend.Contexts;

public class DataContext:DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// 重写SaveChanges方法，在保存前执行全局更新策略
    /// </summary>
    /// <returns>受影响的行数</returns>
    public override int SaveChanges()
    {
        HandleEntityUpdates();
        return base.SaveChanges();
    }

    /// <summary>
    /// 重写SaveChangesAsync方法，在保存前执行全局更新策略
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleEntityUpdates();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 处理实体更新的全局策略
    /// 1. 只更新非null字段（null值不会覆盖数据库中的非null值）
    /// 2. 自动更新UpdateDate字段
    /// </summary>
    private void HandleEntityUpdates()
    {
        // 获取所有被修改的实体
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // 全局更新策略：只更新非null字段
            var originalValues = entry.OriginalValues;  // 数据库中的原始值
            var currentValues = entry.CurrentValues;    // 当前要设置的值

            // 遍历实体的所有属性
            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // 如果当前值为null，但原始值不为null，则保持原始值
                // 这样可以避免null值覆盖数据库中的非null值
                if (property.CurrentValue == null  && originalValues[propertyName] != null)
                {
                    property.CurrentValue = originalValues[propertyName];
                }
            }

            // 自动更新UpdateDate字段（如果实体有这个字段）
            var updateDateProperty = entry.Metadata.FindProperty("UpdateDate");
            if (updateDateProperty != null && updateDateProperty.ClrType == typeof(DateTime))
            {
                entry.Property("UpdateDate").CurrentValue = DateTime.Now;
            }
        }
    }
}