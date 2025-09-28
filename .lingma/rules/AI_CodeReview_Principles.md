# AI Code Review 检查原则（可直接应用版）

## 使用说明
本文档将软件设计原则转化为AI可以直接在code review中应用的检查规则。每个规则包含：
- **检查点**：具体要检查什么
- **代码模式**：好的和坏的代码示例
- **评分标准**：如何判断代码质量
- **修复建议**：发现问题时的改进方向

---

## 1. 单一责任原则 (SRP) 检查

### 检查点
- [ ] 类是否只有一个变化原因？
- [ ] 方法是否只做一件事？
- [ ] 是否存在职责混杂的类？

### 代码模式识别

**❌ 违反SRP的代码模式：**
```csharp
// 反例：职责混杂
class UserService 
{
    public void CreateUser(User user) { /* 创建用户 */ }
    public void SendEmail(string email) { /* 发送邮件 */ }
    public void LogActivity(string activity) { /* 记录日志 */ }
    public string FormatUserData(User user) { /* 格式化数据 */ }
}
```

**✅ 符合SRP的代码模式：**
```csharp
// 正例：职责分离
class UserService 
{
    public void CreateUser(User user) { /* 只负责用户创建 */ }
}

class EmailService 
{
    public void SendEmail(string email) { /* 只负责邮件发送 */ }
}

class ActivityLogger 
{
    public void LogActivity(string activity) { /* 只负责日志记录 */ }
}
```

### 评分标准
- **优秀**：每个类/方法职责单一，命名清晰
- **良好**：大部分职责分离，少量混杂
- **需改进**：职责混杂，一个类承担多个不相关功能

### 修复建议
- 按"变化轴"拆分职责
- 使用"角色/协作者"命名
- 提取公共功能到独立类

---

## 2. 开闭原则 (OCP) 检查

### 检查点
- [ ] 新增功能是否需要修改现有代码？
- [ ] 是否存在大量switch/if-else分支？
- [ ] 是否使用了抽象和多态？

### 代码模式识别

**❌ 违反OCP的代码模式：**
```csharp
// 反例：每次新增折扣都要修改现有代码
class DiscountCalculator 
{
    public decimal CalculateDiscount(Order order) 
    {
        if (order.CustomerType == "VIP") 
            return order.Amount * 0.1m;
        else if (order.CustomerType == "Regular") 
            return order.Amount * 0.05m;
        else if (order.HasCoupon) 
            return 50m;
        // 新增折扣类型需要修改这里
    }
}
```

**✅ 符合OCP的代码模式：**
```csharp
// 正例：对扩展开放，对修改关闭
interface IDiscountPolicy 
{
    decimal CalculateDiscount(Order order);
}

class VipDiscountPolicy : IDiscountPolicy { /* VIP折扣逻辑 */ }
class RegularDiscountPolicy : IDiscountPolicy { /* 普通折扣逻辑 */ }
class CouponDiscountPolicy : IDiscountPolicy { /* 优惠券折扣逻辑 */ }

class DiscountCalculator 
{
    private readonly IDiscountPolicy _policy;
    public DiscountCalculator(IDiscountPolicy policy) => _policy = policy;
    
    public decimal CalculateDiscount(Order order) => _policy.CalculateDiscount(order);
}
```

### 评分标准
- **优秀**：使用抽象和多态，新增功能无需修改现有代码
- **良好**：大部分使用抽象，少量硬编码分支
- **需改进**：大量switch/if-else，每次新增都要修改现有代码

### 修复建议
- 引入抽象接口
- 使用策略模式
- 用配置驱动替代硬编码

---

## 3. 里氏替换原则 (LSP) 检查

### 检查点
- [ ] 子类是否可以替换父类而不破坏功能？
- [ ] 子类是否加强了前置条件？
- [ ] 子类是否削弱了后置条件？

### 代码模式识别

**❌ 违反LSP的代码模式：**
```csharp
// 反例：正方形继承矩形破坏LSP
class Rectangle 
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
}

class Square : Rectangle 
{
    public override int Width 
    { 
        get => base.Width; 
        set { base.Width = value; base.Height = value; } // 破坏矩形行为
    }
    public override int Height 
    { 
        get => base.Height; 
        set { base.Height = value; base.Width = value; } // 破坏矩形行为
    }
}
```

**✅ 符合LSP的代码模式：**
```csharp
// 正例：使用组合代替继承
interface IShape 
{
    int GetArea();
}

class Rectangle : IShape 
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int GetArea() => Width * Height;
}

class Square : IShape 
{
    public int Side { get; set; }
    public int GetArea() => Side * Side;
}
```

### 评分标准
- **优秀**：子类完全可替换父类，行为一致
- **良好**：大部分情况下可替换，少量边界情况
- **需改进**：子类改变了父类行为，不可替换

### 修复建议
- 以行为契约设计继承
- 违反时使用组合代替继承
- 明确前置/后置条件

---

## 4. 接口隔离原则 (ISP) 检查

### 检查点
- [ ] 接口是否过于臃肿？
- [ ] 实现类是否被迫实现不需要的方法？
- [ ] 接口是否按使用场景拆分？

### 代码模式识别

**❌ 违反ISP的代码模式：**
```csharp
// 反例：臃肿接口
interface IWorker 
{
    void Work();
    void Eat();
    void Sleep();
    void Program();
    void Design();
    void Test();
}

class Developer : IWorker 
{
    public void Work() { /* 工作 */ }
    public void Eat() { /* 吃饭 */ }
    public void Sleep() { /* 睡觉 */ }
    public void Program() { /* 编程 */ }
    public void Design() { /* 设计 - 不需要 */ }
    public void Test() { /* 测试 - 不需要 */ }
}
```

**✅ 符合ISP的代码模式：**
```csharp
// 正例：接口隔离
interface IWorker 
{
    void Work();
    void Eat();
    void Sleep();
}

interface IDeveloper 
{
    void Program();
}

interface IDesigner 
{
    void Design();
}

interface ITester 
{
    void Test();
}

class Developer : IWorker, IDeveloper 
{
    // 只实现需要的接口
}
```

### 评分标准
- **优秀**：接口职责单一，实现类只实现需要的方法
- **良好**：大部分接口合理，少量臃肿
- **需改进**：接口臃肿，实现类被迫实现不需要的方法

### 修复建议
- 按使用者视角拆分接口
- 粒度与调用场景一致
- 提供组合接口简化调用

---

## 5. 依赖倒置原则 (DIP) 检查

### 检查点
- [ ] 高层模块是否依赖抽象？
- [ ] 是否使用了依赖注入？
- [ ] 抽象是否不依赖具体实现？

### 代码模式识别

**❌ 违反DIP的代码模式：**
```csharp
// 反例：高层依赖低层
class OrderService 
{
    private SqlServerDatabase _database; // 依赖具体实现
    private SmtpEmailService _emailService; // 依赖具体实现
    
    public void ProcessOrder(Order order) 
    {
        _database.Save(order);
        _emailService.SendConfirmation(order.CustomerEmail);
    }
}
```

**✅ 符合DIP的代码模式：**
```csharp
// 正例：依赖抽象
interface IOrderRepository 
{
    void Save(Order order);
}

interface IEmailService 
{
    void SendConfirmation(string email);
}

class OrderService 
{
    private readonly IOrderRepository _repository; // 依赖抽象
    private readonly IEmailService _emailService; // 依赖抽象
    
    public OrderService(IOrderRepository repository, IEmailService emailService) 
    {
        _repository = repository;
        _emailService = emailService;
    }
    
    public void ProcessOrder(Order order) 
    {
        _repository.Save(order);
        _emailService.SendConfirmation(order.CustomerEmail);
    }
}
```

### 评分标准
- **优秀**：完全依赖抽象，使用依赖注入
- **良好**：大部分依赖抽象，少量直接依赖
- **需改进**：直接依赖具体实现，难以测试和替换

### 修复建议
- 面向接口编程
- 使用构造器注入
- 在组成根装配依赖

---

## 6. DRY原则检查

### 检查点
- [ ] 是否存在重复代码？
- [ ] 重复的逻辑是否被提取？
- [ ] 是否存在"错误的抽象"？

### 代码模式识别

**❌ 违反DRY的代码模式：**
```csharp
// 反例：重复代码
class UserService 
{
    public void ValidateUser(User user) 
    {
        if (string.IsNullOrEmpty(user.Name)) 
            throw new ArgumentException("Name is required");
        if (user.Age < 0) 
            throw new ArgumentException("Age must be positive");
    }
}

class ProductService 
{
    public void ValidateProduct(Product product) 
    {
        if (string.IsNullOrEmpty(product.Name)) 
            throw new ArgumentException("Name is required");
        if (product.Price < 0) 
            throw new ArgumentException("Price must be positive");
    }
}
```

**✅ 符合DRY的代码模式：**
```csharp
// 正例：提取公共逻辑
class ValidationHelper 
{
    public static void ValidateRequired(string value, string fieldName) 
    {
        if (string.IsNullOrEmpty(value)) 
            throw new ArgumentException($"{fieldName} is required");
    }
    
    public static void ValidatePositive(decimal value, string fieldName) 
    {
        if (value < 0) 
            throw new ArgumentException($"{fieldName} must be positive");
    }
}

class UserService 
{
    public void ValidateUser(User user) 
    {
        ValidationHelper.ValidateRequired(user.Name, "Name");
        ValidationHelper.ValidatePositive(user.Age, "Age");
    }
}
```

### 评分标准
- **优秀**：无重复代码，公共逻辑被正确抽象
- **良好**：少量重复，大部分逻辑被提取
- **需改进**：大量重复代码，相同逻辑在多处实现

### 修复建议
- 提炼公共模块
- 使用代码生成/模板
- 避免"错误的抽象"

---

## 7. KISS原则检查

### 检查点
- [ ] 代码是否过于复杂？
- [ ] 是否使用了最简单的解决方案？
- [ ] 是否存在不必要的抽象？

### 代码模式识别

**❌ 违反KISS的代码模式：**
```csharp
// 反例：过度复杂
class SimpleCalculator 
{
    public int Add(int a, int b) 
    {
        var operation = new AdditionOperation();
        var context = new CalculationContext(a, b);
        var strategy = new BinaryOperationStrategy();
        var result = strategy.Execute(operation, context);
        return result.Value;
    }
}
```

**✅ 符合KISS的代码模式：**
```csharp
// 正例：简单直接
class SimpleCalculator 
{
    public int Add(int a, int b) => a + b;
}
```

### 评分标准
- **优秀**：代码简洁明了，使用最简单的解决方案
- **良好**：大部分代码简单，少量复杂
- **需改进**：过度复杂，使用了不必要的抽象

### 修复建议
- 选择简单数据结构
- 避免不必要的元编程
- 消除可选项和约束

---

## 8. YAGNI原则检查

### 检查点
- [ ] 是否实现了当前不需要的功能？
- [ ] 是否存在"预留"的代码？
- [ ] 是否基于真实需求开发？

### 代码模式识别

**❌ 违反YAGNI的代码模式：**
```csharp
// 反例：实现不需要的功能
class UserService 
{
    public void CreateUser(User user) { /* 当前需要 */ }
    public void UpdateUser(User user) { /* 当前需要 */ }
    public void DeleteUser(int id) { /* 当前需要 */ }
    
    // 以下功能当前不需要，违反YAGNI
    public void ArchiveUser(int id) { /* 未来可能需要 */ }
    public void RestoreUser(int id) { /* 未来可能需要 */ }
    public void BulkImportUsers(List<User> users) { /* 未来可能需要 */ }
}
```

**✅ 符合YAGNI的代码模式：**
```csharp
// 正例：只实现当前需要的功能
class UserService 
{
    public void CreateUser(User user) { /* 当前需要 */ }
    public void UpdateUser(User user) { /* 当前需要 */ }
    public void DeleteUser(int id) { /* 当前需要 */ }
    
    // 当真正需要时再添加其他功能
}
```

### 评分标准
- **优秀**：只实现当前需要的功能
- **良好**：大部分功能是必需的，少量预留
- **需改进**：大量"预留"功能，过度设计

### 修复建议
- 以用户价值驱动
- 延迟可变点
- 使用特性开关

---

## 9. 深度vs广度检查（软件设计哲学）

### 检查点
- [ ] 是否存在"浅层类"？
- [ ] 抽象是否提供实质性价值？
- [ ] 接口数量是否过多？

### 代码模式识别

**❌ 违反深度原则的代码模式：**
```csharp
// 反例：浅层类，只是简单转发
class UserDataAccessor 
{
    private Database _db;
    public User GetUser(int id) => _db.GetUser(id);
}

class UserValidator 
{
    public bool IsValid(User user) => user != null;
}

class UserFormatter 
{
    public string Format(User user) => user.ToString();
}
```

**✅ 符合深度原则的代码模式：**
```csharp
// 正例：深层抽象，提供实质性价值
class UserRepository 
{
    private readonly Database _db;
    private readonly Cache _cache;
    private readonly AuditLogger _logger;
    
    public User GetUser(int id) 
    {
        // 缓存检查
        var cached = _cache.Get<User>($"user_{id}");
        if (cached != null) return cached;
        
        // 数据库查询
        var user = _db.GetUser(id);
        if (user == null) return null;
        
        // 缓存存储
        _cache.Set($"user_{id}", user, TimeSpan.FromMinutes(30));
        
        // 审计日志
        _logger.LogAccess(id, "GetUser");
        
        return user;
    }
}
```

### 评分标准
- **优秀**：深层抽象，每个类提供实质性价值
- **良好**：大部分抽象有深度，少量浅层
- **需改进**：大量浅层类，只是简单转发调用

### 修复建议
- 创建深层抽象
- 减少接口数量
- 每个抽象提供实质性价值

---

## 10. 复杂性管理检查

### 检查点
- [ ] 代码的认知负荷是否合理？
- [ ] 是否消除了不必要的复杂性？
- [ ] 复杂性是否被正确封装？

### 代码模式识别

**❌ 高复杂性的代码模式：**
```csharp
// 反例：高认知负荷
public void ProcessOrder(Order order) 
{
    if (order != null && order.Items != null && order.Items.Count > 0) 
    {
        var total = 0m;
        foreach (var item in order.Items) 
        {
            if (item != null && item.Price > 0) 
            {
                var discount = 0m;
                if (order.Customer != null && order.Customer.IsVip) 
                {
                    discount = item.Price * 0.1m;
                }
                else if (order.Customer != null && order.Customer.HasCoupon) 
                {
                    discount = 50m;
                }
                total += item.Price - discount;
            }
        }
        // 更多复杂逻辑...
    }
}
```

**✅ 低复杂性的代码模式：**
```csharp
// 正例：低认知负荷
public void ProcessOrder(Order order) 
{
    var total = CalculateOrderTotal(order);
    ApplyDiscounts(order, ref total);
    ProcessPayment(order, total);
    SendConfirmation(order);
}

private decimal CalculateOrderTotal(Order order) 
{
    return order.Items.Sum(item => item.Price);
}

private void ApplyDiscounts(Order order, ref decimal total) 
{
    if (order.Customer.IsVip) 
        total *= 0.9m;
    else if (order.Customer.HasCoupon) 
        total -= 50m;
}
```

### 评分标准
- **优秀**：认知负荷低，逻辑清晰
- **良好**：大部分逻辑简单，少量复杂
- **需改进**：高认知负荷，逻辑复杂难懂

### 修复建议
- 消除不必要的复杂性
- 封装复杂性
- 通过抽象隐藏复杂性

---

## AI Code Review 检查清单

### 快速检查（必检项）
- [ ] **SRP**：类/方法是否职责单一？
- [ ] **OCP**：新增功能是否需要修改现有代码？
- [ ] **LSP**：子类是否可以替换父类？
- [ ] **ISP**：接口是否过于臃肿？
- [ ] **DIP**：是否依赖抽象而非具体实现？
- [ ] **DRY**：是否存在重复代码？
- [ ] **KISS**：是否使用了最简单的解决方案？
- [ ] **YAGNI**：是否实现了当前不需要的功能？
- [ ] **深度**：是否存在浅层类？
- [ ] **复杂性**：代码的认知负荷是否合理？

### 详细检查（可选）
- [ ] 命名是否清晰表达意图？
- [ ] 函数是否保持单一抽象层级？
- [ ] 是否存在"火车式调用"？
- [ ] 错误处理是否完善？
- [ ] 测试覆盖率是否足够？
- [ ] 文档是否与代码同步？

### 评分标准
- **优秀 (9-10分)**：完全符合原则，代码质量高
- **良好 (7-8分)**：大部分符合原则，少量问题
- **需改进 (5-6分)**：存在明显问题，需要重构
- **不合格 (0-4分)**：严重违反原则，需要重写

### 修复优先级
1. **高优先级**：违反SRP、DIP、高复杂性
2. **中优先级**：违反OCP、DRY、深度原则
3. **低优先级**：违反KISS、YAGNI、命名问题

---

## 使用建议

1. **自动化检查**：将检查点集成到CI/CD流程
2. **渐进式改进**：优先修复高优先级问题
3. **团队培训**：定期review检查结果，提升团队技能
4. **工具支持**：使用静态分析工具辅助检查
5. **持续优化**：根据项目特点调整检查标准

这个文档可以直接用于AI code review，每个检查点都有明确的判断标准和修复建议。
