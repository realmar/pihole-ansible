# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config| 
  config.vm.box = "ubuntu/bionic64"
  config.vm.hostname = "pihole-dev"
  # config.vm.network "public_network", :mac => "080027370D99"
  config.vm.network "private_network", ip: "192.168.250.102"
  config.vm.synced_folder ".", "/vagrant", disabled: true

  config.vm.provision "shell" do |s|
    ssh_pub_key = File.readlines("#{Dir.home}/.ssh/id_rsa.pub").first.strip
    s.inline = <<-SHELL
      apt-get update
      apt-get install -y python

      mkdir -p /root/.ssh
      chmod 700 /root/.ssh

      echo #{ssh_pub_key} >> /root/.ssh/authorized_keys

      chmod 600 /root/.ssh/authorized_keys
    SHELL
  end
end
