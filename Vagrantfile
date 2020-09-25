# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  # config.vm.box = "debian/buster64"
  config.vm.box = "ubuntu/bionic64"
  config.vm.hostname = "pihole-dev"
  # config.vm.network "public_network", :mac => "080027370D99"
  config.vm.network "private_network", ip: "192.168.250.102"
  config.vm.synced_folder ".", "/vagrant", disabled: true
  config.disksize.size = '50GB'

  config.vm.provider "virtualbox" do |v|
    v.memory = 16384
    v.cpus = 12
  end

  config.vm.provision "shell" do |s|
    ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa.pub").first.strip
    nopass_ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa_nopassphrase.pub").first.strip
    authorized_keys = "/root/.ssh/authorized_keys"

    s.inline = <<-SHELL
      apt-get update
      apt-get install -y python python-pip python3 python3-pip

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
