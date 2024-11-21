<template>
   <main>
     <h2 class="page-name">Dodaj nowy post</h2>
     <div class="dropdown-section">
       <label v-for="(options, index) in tagOptions" :key="index">
         <select v-model="selectedTags[index]" @change="addTag(index)">
           <option disabled value="">Wybierz...</option>
           <option v-for="option in options" :key="option">{{ option }}</option>
         </select>
       </label>
     </div>
 
     <div class="tags-display">
       <div
         v-for="(tag, index) in tags"
         :key="index"
         class="tag"
         @mouseenter="hoveredIndex = index"
         @mouseleave="hoveredIndex = null"
       >
         {{ tag }}
         <span
           v-if="hoveredIndex === index"
           class="remove-icon"
           @click="removeTag(index)"
         >
           ✖
         </span>
       </div>
     </div>
 
     <div class="buttons">
       <button @click="$emit('goBack')" class="back-button">Powrót</button>
       <button @click="publish" class="publish-button">Publikuj</button>
     </div>
   </main>
 </template>
 
 <script>
 export default {
   name: "TagSelectionPage",
   data() {
     return {
       tagOptions: [
         ["Element UI", "Styl", "Kolor"],
         ["Landing", "Block", "Layout", "Flat"],
         ["Printing", "Font"],
       ],
       selectedTags: ["", "", ""],
       tags: [],
       hoveredIndex: null,
     };
   },
   methods: {
     addTag(index) {
       const newTag = this.selectedTags[index];
       if (newTag && !this.tags.includes(newTag)) {
         this.tags.push(newTag);
         this.selectedTags[index] = "";
       }
     },
     removeTag(index) {
       this.tags.splice(index, 1);
     },
     publish() {
       alert("Post opublikowany!");
     },
   },
 };
 </script>
 
 <style scoped>

 .tag {
   display: inline-block;
   background: #ddd;
   margin: 5px;
   padding: 5px 10px;
   border-radius: 20px;
   position: relative;
   transition: background 0.3s ease;
 }
 
 .tag:hover {
   background: #bbb;
 }
 
 .remove-icon {
   margin-left: 10px;
   cursor: pointer;
 }
 
 .buttons {
   margin-top: 20px;
   display: flex;
   justify-content: space-between;
 }


.upload-post {
  padding: 30px;
  max-width: 80vw;
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: 1fr 2fr 50px;
  grid-column-gap: 80px;
  grid-row-gap: 40px;
}

.file-upload {
  grid-area: 1 / 1 / 3 / 2;
}

.title-input {
  grid-area: 1 / 2 / 2 / 3;

  input {
    width: 100%;
  }
}

.description-input {
  grid-area: 2 / 2 / 3 / 3;
  display: flex;
  flex-direction: column;
  textarea {
    width: 100%;
    height: 100%;
  }
}

.button-continue {
  grid-area: 3 / 2 / 4 / 3;
  justify-self: right;
}

.file-upload {
  border: 2px dashed rgba(255, 255, 255, 0.5);
  border-radius: 10px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.3s ease-in-out;
}

.file-upload.active {
  border-color: #007bff;
}

.file-upload .file-content {
  cursor: pointer;
}

.file-upload .placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.file-upload .placeholder span {
  font-size: 3rem;
  color: rgba(255, 255, 255, 0.7);
}

.file-upload .placeholder p {
  margin-top: 10px;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.7);
}

.file-upload .remove-file {
  background: none;
  border: none;
  color: red;
  font-size: 1.2rem;
  cursor: pointer;
  margin-top: 10px;
}

.hidden-input {
  display: none;
}

/* Button Style */
.publish-button {
  background: linear-gradient(135deg, #32334a, #1a1b2d);
  border: none;
  border-radius: 12px;
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
  cursor: pointer;
  padding: 12px 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  aspect-ratio: 85/35;
}

.publish-button:hover {
  background: linear-gradient(135deg, #404158, #28293d);
}

.publish-button:active {
  transform: translateY(2px);
}

.button-continue {
  display: flex;
  justify-content: flex-end;
}

.back-button {
  background: linear-gradient(135deg, #32334a, #1a1b2d);
  border: none;
  border-radius: 12px;
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
  cursor: pointer;
  padding: 12px 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3);
  aspect-ratio: 85/35;
}

.back-button:hover {
  background: linear-gradient(135deg, #404158, #28293d);
}

.back-button:active {
  transform: translateY(2px);
}

.button-continue {
  display: flex;
  justify-content: flex-end;
}


 </style>
 