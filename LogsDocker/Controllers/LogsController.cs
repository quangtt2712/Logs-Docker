using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DockerLogsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly DockerClient _dockerClient;

        public LogsController()
        {
            // Determine the Docker URI based on the OS platform
            string dockerUri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";

            _dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
        }

        [HttpGet("{containerId}/logs")]
        public async Task<IActionResult> GetLogs(string containerId)
        {
            try
            {
                var parameters = new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Follow = false
                };

                using (var logStream = await _dockerClient.Containers.GetContainerLogsAsync(containerId, parameters))
                using (var reader = new StreamReader(logStream, Encoding.UTF8))
                {
                    var log = await reader.ReadToEndAsync();
                    return Ok(log);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{containerId}/restart")]
        public async Task<IActionResult> RestartContainer(string containerId)
        {
            try
            {
                await _dockerClient.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters());

                return Ok(new { message = "Container restarted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
