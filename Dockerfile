ARG BASE_IMAGE=quay.io/jupyter/pytorch-notebook:x86_64-cuda12-ubuntu-22.04

FROM ${BASE_IMAGE}

# Switch to root for linux updates and installs
USER root
WORKDIR /root

# Install Jupyter Desktop Dependencies
RUN apt-get -y update \
 && apt-get -y install \
    dbus-x11 \
    xfce4 \
    xfce4-panel \
    xfce4-session \
    xfce4-settings \
    xorg \
    xubuntu-icon-theme \
    tigervnc-standalone-server \
    tigervnc-xorg-extension \
    gnupg \ 
    software-properties-common

# Unity Hub Dependencies
RUN wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | tee /usr/share/keyrings/Unity_Technologies_ApS.gpg
RUN sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'
RUN add-apt-repository ppa:mozillateam/ppa

# Install Unity Hub and Firefox-ESR (Used for Unity Hub authentication)
RUN apt-get -y update \
 && apt-get -y install \
    unityhub \
    firefox-esr \
 && apt clean \
 && rm -rf /var/lib/apt/lists/* \
 && fix-permissions "${CONDA_DIR}" \
 && fix-permissions "/home/${NB_USER}"

# Switch back to notebook user
USER $NB_USER
WORKDIR /home/${NB_USER}

# Install Jupyter Desktop
RUN mamba install -y -q -c manics websockify
RUN pip install jupyter-remote-desktop-proxy
