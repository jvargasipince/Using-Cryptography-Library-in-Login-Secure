create table users(
id int identity(1,1) primary key,
username nvarchar(50) unique,
salt varbinary(max) not null,
pass varbinary(max) not null
)
go

select * from users
GO
