# MatchingSystem
Dotnet大作业撮合系统（注册，登录，角色，权限部分）

## 接口列表(Controllers,Services)：

### - 身份认证：
1. 用户注册（实现）
2. 用户登录（实现）
3. 注销登录（实现）
4. 踢除用户（实现）
5. 验证登录状态（实现）
6. 忘记密码（实现）

### - 用户接口：
1. 获取用户信息（实现）
....

## Models:

### Requests: 接口所需或返回内容类
### tables：表（Dblite）
### JwtBody（Jwt内容）

## tips：
注册时传输sha1散列后的密码，修改密码一致，其余端口不会再出现和密码有关的数据。

## 密码加密方式：
构建签名串：
- HTTP请求方法\n
- /api/authorize/login\n
- 请求时间戳\n
- 请求随机串\n
- 登录令牌\n
- 登录密码用SHA1散列
- 所有内容哪怕是空的也要加\n。但最后一行不需要加\n。

拼好的字符串 sha256 散列一次

## 使用Jwt实现accesstoken，redis动态储存

          
