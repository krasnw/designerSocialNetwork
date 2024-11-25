<script>
export default {
  name: "TaskView",
  props: {
    username: {
      type: String,
      required: true
    },
    userProfilePicture: {
      type: String,
      required: true
    },
    description: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      reply: false
    };
  },
  methods: {
    replyTask() {
      this.reply = !this.reply;
    }
  }
};
</script>

<template>
  <article class="background task" :class="{ 'task-with-reply': reply }">
    <span class="task-info">
      <img class="profile-picture" :src="userProfilePicture" alt="Profile Picture">
      <h4 class="username">{{ username }}</h4>
    </span>
    <p class="task-description">{{ description }}</p>

    <transition name="fade">
      <textarea v-show="reply" class="reply-textarea" placeholder="reply to the task"></textarea>
    </transition>

    <span class="control-buttons">
      <button class="delete-button button" v-if="!reply">Usuń</button>
      <button class="delete-button button" @click="reply = false" v-else>Anuluj</button>

      <button @click="replyTask" class="accept-button button" v-if="!reply">Odpowiedź</button>
      <button @click="replyTask" class="accept-button button" v-else>Wyslij</button>
    </span>
  </article>
</template>

<style scoped>
.task {
  padding: 30px 40px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
  transition: all 0.3s ease;
  overflow: hidden;
}

.username {
  font-weight: 700;
  font-size: 21px;
}

.task-info {
  display: flex;
  align-items: center;
  gap: 18px;
}

.profile-picture {
  width: 40px;
  height: 40px;
  border: 0.5px solid white;
  padding: 2px;
  border-radius: 50%;
}

.task-description {
  font-weight: 400;
  font-size: 18px;
  margin-top: 16px;
}

.reply-textarea {
  margin-top: 16px;
  width: 100%;
  height: 150px;
  transition: all 0.3s ease;
}

.control-buttons {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
  gap: 20px;
}

.delete-button {
  background: rgba(255, 57, 57, 0.40);
  border-color: rgba(255, 94, 94, 0.60);
}

.accept-button {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.60);
}

.fade-enter-active {
  transition: all 0.5s ease;
  max-height: 150px;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(-20px);
  max-height: 0;
}
</style>