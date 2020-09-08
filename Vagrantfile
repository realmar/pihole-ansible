# -*- mode: ruby -*-
# vi: set ft=ruby :

def setup_node(node, num)
  node.vm.box = "ubuntu/bionic64"
  node.vm.hostname = "pihole-dev-" + num
  node.vm.network "private_network", ip: "192.168.250.1" + num
  node.vm.synced_folder ".", "/vagrant", disabled: true
end

Vagrant.configure("2") do |config|
  config.disksize.size = '50GB'

  config.vm.define "node01" do |node| setup_node(node, "01") end
  config.vm.define "node02" do |node| setup_node(node, "02") end
  config.vm.define "node03" do |node| setup_node(node, "03") end

  config.vm.provider "virtualbox" do |v|
    v.memory = 2048
    v.cpus = 4
  end

  config.vm.provision "shell" do |s|
    ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa.pub").first.strip
    s.inline = <<-SHELL
      apt-get update
      apt-get install -y python python-pip python3 python3-pip

      mkdir -p /root/.ssh
      chmod 700 /root/.ssh

      echo #{ssh_pub_key} > /root/.ssh/authorized_keys

      chmod 600 /root/.ssh/authorized_keys
    SHELL
  end
end
