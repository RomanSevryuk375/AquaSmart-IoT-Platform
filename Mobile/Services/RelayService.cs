using Mobile.Models;
using Mobile.Services.Dto.Device;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class RelayService : IRelayService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<RelayModel> _relays = new();

    public RelayService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<RelayDto>>(ApiConfiguration.Routes.Relays, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var backendRelays = response.Data.Select((dto, index) => new RelayModel
                {
                    Id = dto.Id,
                    AquariumId = aquariumId,
                    ControllerId = dto.ControllerId,
                    Name = dto.Name,
                    IsOn = dto.IsActive,
                    PinChannel = index + 1,
                    Type = dto.Purpose switch
                    {
                        0 => AquariumDeviceType.FilterPump,
                        1 => AquariumDeviceType.Heater,
                        2 => AquariumDeviceType.Lighting,
                        3 => AquariumDeviceType.Aerator,
                        4 => AquariumDeviceType.Feeder,
                        _ => AquariumDeviceType.FilterPump
                    },
                    ScheduleInfo = dto.IsManual ? LocalizationService.Instance["Relay_ManualMode"] : LocalizationService.Instance["Relay_Scheduled"]
                }).ToList();

                lock (_syncLock)
                {
                    var localOnly = _relays
                        .Where(r => r.AquariumId == aquariumId && !backendRelays.Any(b => b.Id == r.Id))
                        .ToList();

                    _relays.RemoveAll(r => r.AquariumId == aquariumId);
                    _relays.AddRange(backendRelays);
                    _relays.AddRange(localOnly);

                    return _relays.Where(x => x.AquariumId == aquariumId).ToList();
                }
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _relays.Where(x => x.AquariumId == aquariumId).ToList();
        }
    }

    public async Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default)
    {
        try
        {
            int purpose = relay.Type switch
            {
                AquariumDeviceType.FilterPump => 0,
                AquariumDeviceType.Heater => 1,
                AquariumDeviceType.Lighting => 2,
                AquariumDeviceType.Aerator => 3,
                AquariumDeviceType.Feeder => 4,
                _ => 0
            };

            var request = new AddRelayRequestDto(
                relay.ControllerId,
                null,
                relay.Name,
                4, // GPIO
                $"GPIO{relay.PinChannel}",
                true,
                purpose,
                relay.IsOn,
                true);

            var response = await _apiClient.PostAsync<AddRelayRequestDto, RelayCreatedResponse>(
                ApiConfiguration.Routes.Relays, request, cancellationToken);

            if (response.IsSuccess && response.Data != null && response.Data.Id != Guid.Empty)
            {
                relay.Id = response.Data.Id;
            }
        }
        catch
        {
        }

        if (relay.Id == Guid.Empty)
        {
            relay.Id = Guid.NewGuid();
        }

        lock (_syncLock)
        {
            var existingIdx = _relays.FindIndex(r => r.Id == relay.Id);
            if (existingIdx >= 0)
            {
                _relays[existingIdx] = relay;
            }
            else
            {
                _relays.Add(relay);
            }
        }

        return relay;
    }

    public async Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.Commands}/toggle-state/{relayId}";
            await _apiClient.PostAsync(route, new { }, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var relay = _relays.FirstOrDefault(x => x.Id == relayId);
            if (relay != null)
            {
                relay.IsOn = !relay.IsOn;
                return true;
            }
        }

        return true;
    }

    public async Task<bool> DeleteRelayAsync(Guid relayId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.Relays}/{relayId}";
            await _apiClient.DeleteAsync(route, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _relays.RemoveAll(r => r.Id == relayId);
        }

        return true;
    }

    public void RemoveRelaysForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _relays.RemoveAll(r => r.AquariumId == aquariumId);
        }
    }

    public int GetRelaysCount(Guid aquariumId)
    {
        lock (_syncLock)
        {
            return _relays.Count(r => r.AquariumId == aquariumId);
        }
    }

    public RelayModel? GetFirstRelayForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            return _relays.FirstOrDefault(r => r.AquariumId == aquariumId);
        }
    }
}
