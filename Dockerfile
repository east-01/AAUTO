ARG BASE_IMAGE=ghcr.io/selkies-project/nvidia-glx-desktop:latest

FROM ${BASE_IMAGE}

# Switch to root for linux updates and installs
USER root
WORKDIR /root

# Install Jupyter Desktop Dependencies
RUN apt-get -y update \
 && apt-get -y install \
    gnupg \ 
    software-properties-common

# Unity Hub Dependencies
RUN wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | tee /usr/share/keyrings/Unity_Technologies_ApS.gpg
RUN sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

# Install Unity Hub
RUN apt-get -y update \
 && apt-get -y --no-install-recommends install \
    unityhub || : \
 && apt clean \
 && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://code-server.dev/install.sh | sh

# Switch back to notebook user
USER ubuntu
WORKDIR /home/ubuntu

RUN curl -L -O "https://github.com/conda-forge/miniforge/releases/latest/download/Miniforge3-$(uname)-$(uname -m).sh" \
 && bash Miniforge3-$(uname)-$(uname -m).sh -bf

# Add mlagents env
COPY mlagents.yaml mlagents.yaml
RUN /home/ubuntu/miniforge3/bin/mamba env create -f mlagents.yaml || :
   #  && rm mlagents.yaml