class AspDebugbar {

    static instance = new AspDebugbar();
    _minimized;
    _maximized;
    _elementReference;
    _firstLoad = true;
    _toggleIconImageReference;
    _maximizeCutOff;
    _contentContainerReference;
    _debugbarHeight;
    
    constructor() {
        this._debugbarHeight = 38.6;
        this._minimized = window.innerHeight-this._debugbarHeight;
        this._maximized = window.innerHeight/1.5;
        this._maximizeCutOff = window.innerHeight-60;
        this._elementReference = document.getElementById("debug");
        this._contentContainerReference = document.getElementById("contentContainer");
        this._toggleIconImageReference = document.getElementById("toggleIconImage");
        
        this.DragElement(document.getElementById("debug"));

        this.SwitchToView("message");
    }
    
    ToggleMinMax(){
        
        let y = this._elementReference.style.top.substring(0,this._elementReference.style.top.length-2);

        if(y >= this._maximizeCutOff){
            this._elementReference.style.top = this._maximized+"px";
        }else{
            this._elementReference.style.top = this._minimized+"px";
        }
        
        this.UpdateToggleButtonImage();
    }
    
    SwitchToView(view){
        
        if(this._elementReference.style.top === this._minimized){
            this._elementReference.style.top = this._maximized+"px";
        }
        
        let content = view+"Content";
        
        this._contentContainerReference.childNodes.forEach(n =>{
            if(n.nodeName === "SECTION") {
                if (n.id === content) {
                    n.style.display = "block";
                    document.getElementById(view + "Button").classList.add("active-content");

                    let y = this._elementReference.style.top.substring(0, this._elementReference.style.top.length - 2);

                    if (y >= this._maximizeCutOff) {
                        this._elementReference.style.top = this._maximized + "px";
                        this.UpdateContentContainerHeight(y);
                    }

                } else {
                    n.style.display = "none";
                }
            }
            
        });
        
        if(this._firstLoad){
            this._firstLoad = false;
            this._elementReference.style.top = this._minimized+"px";
        }
        
        this.UpdateToggleButtonImage();
        this.UpdateMenuActiveLink(view+"Button");
    }
    
    UpdateToggleButtonImage(){

        let y = this._elementReference.style.top.substring(0,this._elementReference.style.top.length-2);

        if(y < this._maximizeCutOff){
            this._toggleIconImageReference.src = "/images/close-icon.png";
        }else{
            this._toggleIconImageReference.src = "/images/maximise-icon.png";
        }
        
        this.UpdateContentContainerHeight(y);
    }
    
    UpdateContentContainerHeight(y){
        this._contentContainerReference.style.height = (window.innerHeight-y-this._debugbarHeight)+"px";
    }
    
    UpdateMenuActiveLink(active){
        
        document.getElementById("debugNavigationContainer").childNodes.forEach(n =>{
            if (n.nodeName === "A" && n.id !== active) {
                n.classList.remove("active-content");
            }
        })
    }
    
    ToggleTableContent(id){
        let element = document.getElementById(id);
        if(element.style.display === "block"){
            element.style.display = "none";
        }else{
            element.style.display = "block";
        }
    }

    //---------- START CITED CODE ----------\\
    
    //W3Schools (no date) How to - create a draggable HTML element,
    // How To Create a Draggable HTML Element. 
    // Available at: https://www.w3schools.com/howto/howto_js_draggable.asp 
    // (Accessed: 16 July 2023). 
    DragElement(elmnt) {
        var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
        if (document.getElementById(elmnt.id + "header")) {
            // if present, the header is where you move the DIV from:
            document.getElementById(elmnt.id + "header").onmousedown = dragMouseDown;
        } else {
            // otherwise, move the DIV from anywhere inside the DIV:
            elmnt.onmousedown = dragMouseDown;
        }

        function dragMouseDown(e) {
            e = e || window.event;
            e.preventDefault();
            // get the mouse cursor position at startup:
            pos3 = e.clientX;
            pos4 = e.clientY;
            document.onmouseup = closeDragElement;
            // call a function whenever the cursor moves:
            document.onmousemove = elementDrag;
        }

        function elementDrag(e) {
            e = e || window.event;
            e.preventDefault();
            // calculate the new cursor position:
            pos1 = pos3 - e.clientX;
            pos2 = pos4 - e.clientY;
            pos3 = e.clientX;
            pos4 = e.clientY;
            // set the element's new position:
            var newY = (elmnt.offsetTop - pos2);

            if (newY < (window.innerHeight - AspDebugbar.instance._debugbarHeight) && newY > 0) {
                elmnt.style.top = newY + "px";
                AspDebugbar.instance.UpdateToggleButtonImage();
                AspDebugbar.instance.UpdateContentContainerHeight(newY);
            }
        }

        function closeDragElement() {
            // stop moving when mouse button is released:
            document.onmouseup = null;
            document.onmousemove = null;
        }
    }

    //---------- END CITED CODE ----------\\
    
}