yum update -y

# Cài đặt máy chủ Apache
sudo yum -y install httpd mod_ssl mod_rewrite

# Cài đặt máy chủ Nginx

echo '
[nginx]
name=nginx repo
baseurl=http://nginx.org/packages/mainline/centos/7/$basearch/
gpgcheck=0
enabled=1
' > /etc/yum.repos.d/nginx.repo

sudo yum install nginx -y

# Tat SELinux cua CentOS
setenforce 0
sed -i --follow-symlinks 's/^SELINUX=enforcing/SELINUX=disabled/' /etc/sysconfig/selinux

# Đổi root password thành 123 và cho phép login SSH qua root
echo "123" | passwd --stdin root
sed -i 's/^PasswordAuthentication no/PasswordAuthentication yes/' /etc/ssh/sshd_config
systemctl reload sshd

# Cài đặt .NET 6
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-6.0 -y
sudo yum install aspnetcore-runtime-6.0  -y

# Cài đặt MS SQL Server 2019 trên CentOS 7
sudo curl -o /etc/yum.repos.d/mssql-server.repo https://packages.microsoft.com/config/rhel/7/mssql-server-2019.repo
sudo yum install -y mssql-server