<script>
import defaultAvatar from "@/assets/Images/avatar.png";
import { imageDirectory } from "@/services/constants";
import { taskService } from "@/services/task";

export default {
  name: "TaskView",
  props: {
    task: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      isAccepted: false,
      isDeleted: false,
      isAllowed: true,
      deleteTimer: null,
      deleteCountdown: 10
    }
  },
  methods: {
    async acceptTask() {
      try {
        await taskService.acceptRequest(this.task.id);
        this.isAccepted = true;
      } catch (error) {
        if (error.status === 400) {
          this.isAllowed = false;
        } else {
          this.$router.push(`/error/${error.status}`);
        }
      }
    },
    deleteTask() {
      this.deleteCountdown = 10;
      this.isDeleted = true;
      this.deleteTimer = setInterval(() => {
        this.deleteCountdown--;
        if (this.deleteCountdown === 0) {
          this.confirmDelete();
        }
      }, 1000);
    },
    cancelDelete() {
      clearInterval(this.deleteTimer);
      this.isDeleted = false;
      this.deleteCountdown = 10;
    },
    async confirmDelete() {
      try {
        clearInterval(this.deleteTimer);
        await taskService.rejectRequest(this.task.id);
        this.$emit('task-deleted', this.task.id);
      } catch (error) {
        this.$router.push(`/error/${error.status}`);
      }
    },
    goToProfile() {
      this.$router.push(`/${this.task.senderProfile.username}/portfolio`);
    },
    goToChat() {
      this.$router.push(`/conversations/${this.task.senderProfile.username}`);
    },
    imagePathHandler(image) {
      return image ? imageDirectory + image : defaultAvatar;
    }
  }
};
</script>

<template>
  <p class="message background" v-if="isAccepted">The task has been <span class="green-gradient">accepted</span>
    <br>Go to <a @click="goToChat" class="chat-link blue-gradient">chat</a>
  </p>
  <div class="delete-message" v-if="isDeleted">
    <p class="message background">
      The task has been <span class="red-gradient">deleted</span>
    </p>
    <button @click="cancelDelete" class="delete-button button">
      Undo ({{ deleteCountdown }})
    </button>
  </div>
  <article v-if="!(isAccepted || isDeleted)" class="task background">
    <span class="task-info" @click="goToProfile">
      <img class="profile-picture no-select" :src="imagePathHandler(task.senderProfile.profileImage)"
        alt="Profile Picture">
      <h3 class="username">{{ task.senderProfile.firstName + " " + task.senderProfile.lastName }}</h3>
    </span>
    <p class="task-description">{{ task.description }}</p>
    <span :class="isAllowed ? 'common-control-buttons' : 'warning-control-buttons'">
      <p v-if="!isAllowed" class="warning">This task cannot be started until<br>it is <span
          class="red-gradient">closed</span> with this
        <a @click="goToChat" class="chat-link blue-gradient">user</a>
      </p>
      <span class="control-buttons">
        <button @click="deleteTask" class="delete-button button">Delete</button>
        <button v-if="isAllowed" @click="acceptTask" class="accept-button button">Accept</button>
      </span>
    </span>
  </article>
</template>

<style scoped>
.message {
  padding: 15px;
  width: max-content;
  font-size: 15px;
  font-weight: 600;
}

.warning {
  text-wrap: balance;
}

.delete-message {
  display: flex;
  flex-direction: column;

  gap: 10px;
}

.chat-link {
  cursor: pointer;
  text-decoration: none;
}

.task {
  padding: 20px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
  transition: all 0.3s ease;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: 15px;
  border-radius: 30px;
}

.task-info {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.username:hover {
  color: var(--text-color);
}

.profile-picture {
  width: 35px;
  height: 35px;
  border: 0.5px solid var(--element-border-light-color);
  padding: 2px;
  border-radius: 50%;
}

.task-description {
  text-wrap: balance;
}

.control-buttons {
  display: flex;
  gap: 10px;
}

.common-control-buttons {
  display: flex;
  justify-content: flex-end;
}

.warning-control-buttons {
  display: flex;
  justify-content: space-between;
}
</style>