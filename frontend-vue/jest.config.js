module.exports = {
  testEnvironment: "jsdom",
  transform: {
    "^.+\\.vue$": "@vue/vue3-jest",
    "^.+\\js$": "babel-jest",
  },
  moduleFileExtensions: ["vue", "js", "json"],
  moduleNameMapper: {
    "^@/(.*)$": "<rootDir>/src/$1",
  },
};
