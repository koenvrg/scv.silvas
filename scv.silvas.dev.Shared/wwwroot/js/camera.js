window.cameraHelper = {
    stream: null,

    async startCamera(videoId) {
        try {
            const constraints = {
                video: { facingMode: { ideal: 'environment' }, width: { ideal: 1280 }, height: { ideal: 720 } },
                audio: false
            };
            this.stream = await navigator.mediaDevices.getUserMedia(constraints);
            const video = document.getElementById(videoId);
            if (!video) return false;
            video.srcObject = this.stream;
            await video.play();
            return true;
        } catch (e) {
            console.error('Camera fout:', e);
            return false;
        }
    },

    capturePhoto(videoId, canvasId) {
        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);
        if (!video || !canvas) return null;
        canvas.width = video.videoWidth || 640;
        canvas.height = video.videoHeight || 480;
        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
        return canvas.toDataURL('image/jpeg', 0.85);
    },

    stopCamera() {
        if (this.stream) {
            this.stream.getTracks().forEach(t => t.stop());
            this.stream = null;
        }
    }
};
