// Blazor WASM no puede "descargar" un byte[] directamente: hay que crear un Blob
// y simular el click en un <a download>. Se llama vía IJSRuntime.InvokeVoidAsync.
window.descargarArchivo = (nombreArchivo, tipoMime, bytesBase64) => {
    const bytes = Uint8Array.from(atob(bytesBase64), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: tipoMime });
    const url = URL.createObjectURL(blob);

    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = nombreArchivo;
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);

    URL.revokeObjectURL(url);
};
