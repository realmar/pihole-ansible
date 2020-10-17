# -*- mode: ruby -*-
# vi: set ft=ruby :

def setup_disk(v, name, num)
  unless File.exist?(name)
    v.customize ['createhd', '--filename', name,'--format', 'VDI', '--size', 10 * 1024]
  end
  v.customize ['storageattach', :id, '--storagectl', 'SCSI', '--port', num + 2, '--device', 0, '--type', 'hdd', '--medium', name]
end

def setup_node(node, num, ram)
  node.vm.box = "ubuntu/bionic64"
  node.vm.hostname = "dev-node-" + num
  node.vm.network "private_network", ip: "192.168.250.1" + num
  node.vm.synced_folder ".", "/vagrant", disabled: true

  node.vm.provider "virtualbox" do |v|
    v.memory = ram
    v.cpus = 4

    setup_disk(v, './sdb' + num + '.vdi', 0)
    setup_disk(v, './sdc' + num + '.vdi', 1)
  end
end

Vagrant.configure("2") do |config|
  config.disksize.size = '50GB'

  config.vm.define "node01" do |node| setup_node(node, "01", 2048) end
  config.vm.define "node02" do |node| setup_node(node, "02", 2048) end
  config.vm.define "node03" do |node| setup_node(node, "03", 2048) end
  config.vm.define "node04" do |node| setup_node(node, "04", 2048) end
  config.vm.define "node05" do |node| setup_node(node, "05", 2048) end
  config.vm.define "node06" do |node| setup_node(node, "06", 2048) end

  config.vm.provision "shell" do |s|
    ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa.pub").first.strip
    nopass_ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa_nopassphrase.pub").first.strip
    authorized_keys = "/root/.ssh/authorized_keys"

    s.inline = <<-SHELL
      apt-get update
      apt-get install -y python3 python3-pip python3-apt

      mkdir -p /root/.ssh
      chmod 700 /root/.ssh

      echo #{ssh_pub_key} > #{authorized_keys}
      echo "\n" >> #{authorized_keys}
      echo #{nopass_ssh_pub_key} >> #{authorized_keys}
      echo "\n" >> #{authorized_keys}

      chmod 600 #{authorized_keys}
    SHELL
  end
end
