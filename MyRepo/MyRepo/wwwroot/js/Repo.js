document.addEventListener('DOMContentLoaded', () => {
    const repoItems = document.getElementsByName("_repoItem");
    for (const item of repoItems) {
        new RepoItem(item.id).onEditModeOff();
    }
});

class RepoItem {
    editMode = false;
    id;
    repo;
    buttons = {};
    inputs = {};
    details;

    constructor(id) {
        this.id = id;
        this.repo = document.getElementById(id);

        this.details = document.getElementsByName(id + " d")

        this.inputs = {
            isPublic: document.getElementById(id + " isP"),
            description: document.getElementById(id + " desc")
        }

        this.buttons = {
            downBtn : document.getElementById(id + " down"),
            delBtn: document.getElementById(id + " del"),
            editBtn: document.getElementById(id + " edit"),
            saveBtn: document.getElementById(id + " save")
        }

        this.buttons.delBtn.addEventListener('click', () => this.del(id));
        this.buttons.editBtn.addEventListener('click', () => this.setEditMode());
        this.buttons.saveBtn.addEventListener('click',async () => await this.save());
    }

    del(id) {
        if (!this.editMode) return;

        const t = document.getElementById(id);
        t.setAttribute("class", "srink")

        t.childNodes.forEach(e => {
            e.parentElement.innerHTML = "";
        })
    }

    async save() {
        await fetch("tt@gmail.com/Save", {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "Id": Number(this.id),
                "Description": String(this.inputs.description.value),
                "IsPublic": Boolean(this.inputs.isPublic.checked)
            })
        }).then(r => {
            console.log("-> " + r);
        }).catch(e => {
            console.log(e);
        })

        this.details[0].innerHTML = this.inputs.description.value;
        this.details[1].innerHTML = this.inputs.isPublic.checked;
    }

    setEditMode() {
        if (!this.editMode) {
            this.editMode = true;
            this.onEditModeOn();
        } else {
            this.editMode = false;
            this.onEditModeOff();
        }
    }

    onEditModeOff() {
        this.buttons.saveBtn.style.display = "none";
        this.buttons.delBtn.style.display = "none";
        this.buttons.downBtn.style.display = "block";

        this.inputs.isPublic.style.display = "none";
        this.inputs.description.style.display = "none";

        this.details.forEach(e => {
            e.style.display = "block";
        })
    }

    onEditModeOn() {
        this.buttons.saveBtn.style.display = "block";
        this.buttons.delBtn.style.display = "block";
        this.buttons.downBtn.style.display = "none";

        this.inputs.isPublic.style.display = "block";
        this.inputs.description.style.display = "block";

        this.details.forEach(e => {
            e.style.display = "none";
        })
    }
}